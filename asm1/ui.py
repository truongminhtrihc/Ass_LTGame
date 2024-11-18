from settings import *
from assets_loader import *

def draw_grid():
    # Vẽ các ô trống trước khi zombie xuất hiện
    for row in range(ROWS):
        for col in range(COLS):
            screen.blit(tile_image, (col * CELL_SIZE - 10, row * CELL_SIZE - 10))  # Vẽ ô trống (tile)
            # rect = pygame.Rect(col * CELL_SIZE, row * CELL_SIZE, CELL_SIZE, CELL_SIZE)
            # pygame.draw.rect(screen, BLACK, rect, 1)  # Đặt đường biên màu đen


def draw_hammer(hit=False):
    mouse_pos = pygame.mouse.get_pos()  # Lấy vị trí hiện tại của con trỏ chuột
    if hit:
        # Vẽ cây búa khi đập
        screen.blit(hammer_hit_image, (mouse_pos[0] - 100, mouse_pos[1] - 100))  # Đặt cây búa tại vị trí con trỏ, điều chỉnh offset phù hợp
    else:
        # Vẽ cây búa bình thường
        screen.blit(hammer_image, (mouse_pos[0] - 100, mouse_pos[1] - 100))  # Đặt cây búa tại vị trí con trỏ, điều chỉnh offset phù hợp


def draw_timer(timer):
    timer_text = font.render(f"Time: {int(timer)}", True, BLACK)
    screen.blit(timer_text, (10, 10))

def draw_score(score, misses):
    score_text = font.render(f"Score: {score} Hits / {misses} Misses", True, BLACK)
    screen.blit(score_text, (10, 50))


def button(text, x, y, w, h, inactive_color, active_color, action=None, args=()):
    mouse = pygame.mouse.get_pos()
    click = pygame.mouse.get_pressed()
    
    if x + w > mouse[0] > x and y + h > mouse[1] > y:
        pygame.draw.rect(screen, active_color, (x, y, w, h))
        if click[0] == 1 and action is not None:
            action(*args)
    else:
        pygame.draw.rect(screen, inactive_color, (x, y, w, h))

    text_surf = font.render(text, True, WHITE)
    text_rect = text_surf.get_rect(center=((x + (w // 2)), (y + (h // 2))))
    screen.blit(text_surf, text_rect)

# Thêm biến để quản lý stun
stun_duration = 0.5  # Thời gian stun sau khi hit, tính bằng giây
stun_timer = 0
stun_active = False
last_zombie_position = -1  # Biến để lưu vị trí zombie bị đập trúng, -1 là giá trị khởi tạo
stun_alpha = 0  # Giá trị alpha của stun, ban đầu là 0 (hoàn toàn trong suốt)

def draw_stun(position, alpha):
    """Vẽ hình ảnh stun quanh zombie bị hit với hiệu ứng fade-in"""
    row = position // COLS
    col = position % COLS
    stun_image.set_alpha(alpha)  # Thiết lập độ trong suốt cho stun
    stun_rect = stun_image.get_rect(center=(col * CELL_SIZE + CELL_SIZE // 2, row * CELL_SIZE + CELL_SIZE // 2))
    screen.blit(stun_image, stun_rect)
    