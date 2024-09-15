import pygame
import random
import sys
import os

# Tên file để lưu điểm cao nhất
HIGH_SCORE_FILE = 'high_score.txt'

# Hàm để tải điểm cao nhất từ file
def load_high_score():
    if os.path.exists(HIGH_SCORE_FILE):
        with open(HIGH_SCORE_FILE, 'r') as file:
            try:
                return int(file.read())
            except ValueError:
                return 0
    return 0

# Hàm để lưu điểm cao nhất vào file
def save_high_score(score):
    with open(HIGH_SCORE_FILE, 'w') as file:
        file.write(str(score))
high_score = load_high_score()  # Khởi tạo biến điểm cao nhất từ file
# Khởi tạo Pygame
pygame.init()

# Thiết lập kích thước màn hình
SCREEN_WIDTH, SCREEN_HEIGHT = 800, 800
ROWS, COLS = 4, 4
CELL_SIZE = SCREEN_WIDTH // COLS

# Màu sắc
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)
RED = (255, 0, 0)
GREEN = (0, 255, 0)

# Thiết lập màn hình
screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
pygame.display.set_caption("Đập đầu Zombie")

# Font chữ
font = pygame.font.Font(None, 36)

# Tải hình ảnh
background_image = pygame.image.load('img/background.png')  # Tải hình ảnh background từ file PNG
background_image = pygame.transform.scale(background_image, (SCREEN_WIDTH, SCREEN_HEIGHT))  # Điều chỉnh kích thước background
zombie_image = pygame.image.load('img/zombie3.png')  # Tải hình ảnh zombie từ file PNG
zombie_image = pygame.transform.scale(zombie_image, (CELL_SIZE-10, CELL_SIZE-10))  # Điều chỉnh kích thước zombie phù hợp với ô
tile_image = pygame.image.load('img/tile2.png')  # Tải hình ảnh ô trống từ file PNG
tile_image = pygame.transform.scale(tile_image, (CELL_SIZE+30, CELL_SIZE+30))  # Điều chỉnh kích thước ô trống
hit_sound = pygame.mixer.Sound("music/hit.wav")
miss_sound = pygame.mixer.Sound("music/miss.wav")
gameover_sound = pygame.mixer.Sound("music/gameover.wav")
gamerun_sound = pygame.mixer.Sound("music/gamerun.wav")
clock = pygame.time.Clock()

hammer_image = pygame.image.load('img/hammer.png')
hammer_image = pygame.transform.scale(hammer_image, (200, 200))  # Điều chỉnh kích thước cây búa nếu cần
hammer_hit_image = pygame.image.load('img/hammer.png')
hammer_hit_image = pygame.transform.scale(hammer_hit_image, (250, 250))  # Điều chỉnh kích thước cây búa khi đập

zombie_appearing = False
animation_frame = 0
ANIMATION_FRAMES = 20  # Số lượng frame cho animation
animation_speed = 1.0  # Tốc độ của animation sẽ thay đổi theo cấp độ

# Tải hình ảnh stun
stun_image = pygame.image.load('img/stunnedhit.png')
stun_image = pygame.transform.scale(stun_image, (CELL_SIZE + 20, CELL_SIZE + 20))  # Điều chỉnh kích thước stun

# Thêm biến để quản lý stun
stun_duration = 0.5  # Thời gian stun sau khi hit, tính bằng giây
stun_timer = 0
stun_active = False
last_zombie_position = -1  # Biến để lưu vị trí zombie bị đập trúng, -1 là giá trị khởi tạo
stun_alpha = 0  # Giá trị alpha của stun, ban đầu là 0 (hoàn toàn trong suốt)

def draw_grid():
    # Vẽ các ô trống trước khi zombie xuất hiện
    for row in range(ROWS):
        for col in range(COLS):
            screen.blit(tile_image, (col * CELL_SIZE - 10, row * CELL_SIZE - 10))  # Vẽ ô trống (tile)
            # rect = pygame.Rect(col * CELL_SIZE, row * CELL_SIZE, CELL_SIZE, CELL_SIZE)
            # pygame.draw.rect(screen, BLACK, rect, 1)  # Đặt đường biên màu đen

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

def update_zombie(zombie_timer, level_time):
    global zombie_position, misses
    # Cập nhật vị trí zombie theo thời gian cấp độ
    if zombie_timer > level_time:
        zombie_position = random.randint(0, ROWS * COLS - 1)
        misses += 1  # Tính là một lần miss nếu zombie tự thay đổi vị trí
        return 0  # Reset thời gian
    return zombie_timer

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

def game_intro():
    global high_score
    intro = True
    if not pygame.mixer.get_busy():  # Kiểm tra xem âm thanh có đang phát không
        gamerun_sound.play(-1) 
    while intro:
        screen.fill(WHITE)
        screen.blit(background_image, (0, 0))  # Vẽ background
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                sys.exit()

        # Hiển thị điểm cao nhất ở màn hình chính
        high_score_text = font.render(f"High Score: {high_score}", True, BLACK)
        screen.blit(high_score_text, (SCREEN_WIDTH // 2 - high_score_text.get_width() // 2, SCREEN_HEIGHT // 4))

        button("Start", 150, 350, 150, 50, GREEN, (0, 200, 0), select_level)
        button("Quit", 500, 350, 150, 50, RED, (200, 0, 0), quit_game)

        pygame.display.flip()
        clock.tick(15)


def quit_game():
    save_high_score(0)
    pygame.quit()
    sys.exit()

def end_game():
    global score, high_score

    # Hiển thị kết quả cuối cùng và quay về màn hình chính
    pygame.mouse.set_visible(True)
    screen.fill(WHITE)
    screen.blit(background_image, (0, 0))  # Vẽ background

    # Cập nhật điểm cao nhất nếu điểm hiện tại lớn hơn
    if score > high_score:
        high_score = score
        save_high_score(high_score)

    final_score_text = font.render(f"Final Score: {score} Hits / {misses} Misses", True, GREEN)
    high_score_text = font.render(f"High Score: {high_score}", True, GREEN)
    screen.blit(final_score_text, (SCREEN_WIDTH // 4, SCREEN_HEIGHT // 2))
    screen.blit(high_score_text, (SCREEN_WIDTH // 4, SCREEN_HEIGHT // 2 + 50))
    
    gameover_sound.play()
    pygame.display.flip()
    pygame.time.wait(3000)
    game_intro()  # Quay về màn hình chính

def select_level():
    selecting = True
    while selecting:
        screen.fill(WHITE)
        screen.blit(background_image, (0, 0))  # Vẽ background

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                sys.exit()

        # Thêm các nút lựa chọn cấp độ
        button("Easy", 150, 450, 150, 50, GREEN, (0, 200, 0), game_loop, (2,))
        button("Medium", 325, 450, 150, 50, (255, 255, 0), (200, 200, 0), game_loop, (1.5,))
        button("Hard", 500, 450, 150, 50, RED, (200, 0, 0), game_loop, (0.75,))

        pygame.display.flip()
        clock.tick(15)

def draw_stun(position, alpha):
    """Vẽ hình ảnh stun quanh zombie bị hit với hiệu ứng fade-in"""
    row = position // COLS
    col = position % COLS
    stun_image.set_alpha(alpha)  # Thiết lập độ trong suốt cho stun
    stun_rect = stun_image.get_rect(center=(col * CELL_SIZE + CELL_SIZE // 2, row * CELL_SIZE + CELL_SIZE // 2))
    screen.blit(stun_image, stun_rect)
    
def game_loop(level_time):
    global zombie_position, score, misses, zombie_appearing, animation_frame, animation_speed, stun_active, stun_timer, last_zombie_position, stun_alpha
    pygame.mouse.set_visible(False)
    gamerun_sound.stop()

    zombie_position = random.randint(0, ROWS * COLS - 1)
    last_zombie_position = -1  # Khởi tạo vị trí zombie bị đập
    score = 0
    misses = 0
    timer = 30
    zombie_timer = 0
    zombie_appearing = True
    animation_frame = 0
    animation_speed = 1 / level_time
    ANIMATION_FRAMES = 20
    hammer_hit = False
    stun_active = False
    stun_timer = 0
    stun_alpha = 0  # Khởi tạo giá trị alpha cho stun (hoàn toàn trong suốt)

    while True:
        screen.blit(background_image, (0, 0))  # Vẽ background trước
        draw_grid()  # Vẽ các ô trống

        if zombie_appearing:
            alpha = int(255 * (animation_frame / ANIMATION_FRAMES))
            scale_factor = animation_frame / ANIMATION_FRAMES
            fade_in_zombie(zombie_position, alpha, scale_factor)
        else:
            draw_zombie(zombie_position)

        draw_timer(timer)
        draw_score(score, misses)
        draw_hammer(hammer_hit)
        hammer_hit = False

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                sys.exit()
            elif event.type == pygame.MOUSEBUTTONDOWN and event.button == 1:
                x, y = event.pos
                col = x // CELL_SIZE
                row = y // CELL_SIZE
                clicked_position = row * COLS + col

                hammer_hit = True

                if clicked_position == zombie_position and not zombie_appearing:
                    score += 1
                    stun_active = True  # Kích hoạt stun
                    stun_timer = stun_duration  # Đặt thời gian stun
                    stun_alpha = 0  # Đặt giá trị alpha ban đầu cho stun (hoàn toàn trong suốt)
                    last_zombie_position = zombie_position  # Lưu vị trí zombie vừa bị đập
                    new_position = random.randint(0, ROWS * COLS - 1)
                    zombie_position = new_position
                    hit_sound.play()
                    zombie_appearing = True
                    animation_frame = 0
                    zombie_timer = 0
                else:
                    misses += 1
                    miss_sound.play()

        # Hiển thị stun với fade-in
        if stun_active:
            if stun_alpha < 255:
                stun_alpha += 35  # Tăng dần giá trị alpha để tạo hiệu ứng fade-in
            draw_stun(last_zombie_position, stun_alpha)

        button("End", 650, 10, 100, 50, RED, (200, 0, 0), end_game)

        if timer > 0:
            timer -= 1 / 30
            zombie_timer += 1 / 30
        else:
            end_game()

        if zombie_appearing:
            animation_frame += animation_speed
            if animation_frame >= ANIMATION_FRAMES:
                zombie_appearing = False

        # Chỉ thay đổi vị trí zombie sau khi hiệu ứng stun kết thúc
        if zombie_timer > level_time and not zombie_appearing and not stun_active:
            zombie_position = random.randint(0, ROWS * COLS - 1)
            zombie_appearing = True
            animation_frame = 0
            zombie_timer = 0

        # Cập nhật stun timer
        if stun_active:
            stun_timer -= 1 / 30
            if stun_timer <= 0:
                stun_active = False  # Kết thúc stun sau khi hết thời gian

        pygame.display.flip()
        clock.tick(30)

if __name__ == "__main__":
    game_intro()
