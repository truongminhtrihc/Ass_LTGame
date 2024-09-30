from settings import *
from assets_loader import *

zombie_appearing = False
animation_frame = 0
ANIMATION_FRAMES = 20  # Số lượng frame cho animation
animation_speed = 1.0  # Tốc độ của animation sẽ thay đổi theo cấp độ

def draw_zombie(position):
    row = position // COLS
    col = position % COLS

    # Tính toán vị trí y của zombie xuất hiện từ "lỗ" (tile_image)
    if zombie_appearing:
        y_offset = (CELL_SIZE // 2) * (1 - animation_frame / ANIMATION_FRAMES)  # Zombie xuất hiện từ dưới lên
    else:
        y_offset = 0  # Khi animation kết thúc, zombie đứng yên ở vị trí giữa ô

    # Vẽ hình zombie với y offset từ tile_image
    zombie_rect = zombie_image.get_rect(midbottom=(col * CELL_SIZE + CELL_SIZE // 2, (row * CELL_SIZE) + CELL_SIZE + y_offset))
    
    screen.blit(zombie_image, zombie_rect)

def fade_in_zombie(position, alpha, scale_factor):
    row = position // COLS
    col = position % COLS

    # Tạo bản sao của zombie và chỉnh kích thước
    scaled_zombie = pygame.transform.scale(zombie_image, 
                                           (int(CELL_SIZE * scale_factor), int(CELL_SIZE * scale_factor)))
    scaled_zombie.set_alpha(alpha)

    # Tính toán vị trí zombie để xuất hiện ở giữa ô
    zombie_rect = scaled_zombie.get_rect(center=(col * CELL_SIZE + CELL_SIZE // 2, row * CELL_SIZE + CELL_SIZE // 2))
    
    screen.blit(scaled_zombie, zombie_rect)

def update_zombie(zombie_timer, level_time):
    global zombie_position, misses
    # Cập nhật vị trí zombie theo thời gian cấp độ
    if zombie_timer > level_time:
        zombie_position = random.randint(0, ROWS * COLS - 1)
        misses += 1  # Tính là một lần miss nếu zombie tự thay đổi vị trí
        return 0  # Reset thời gian
    return zombie_timer
