import pygame
import random
import sys

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
zombie_image = pygame.image.load('img/zombie.png')  # Tải hình ảnh zombie từ file PNG
zombie_image = pygame.transform.scale(zombie_image, (CELL_SIZE, CELL_SIZE))  # Điều chỉnh kích thước zombie phù hợp với ô

clock = pygame.time.Clock()

def draw_grid():
    # Vẽ các đường ranh giới của ma trận 4x4
    for row in range(ROWS):
        for col in range(COLS):
            rect = pygame.Rect(col * CELL_SIZE, row * CELL_SIZE, CELL_SIZE, CELL_SIZE)
            pygame.draw.rect(screen, BLACK, rect, 1)  # Đặt đường biên màu đen

def draw_zombie(position):
    row = position // COLS
    col = position % COLS
    screen.blit(zombie_image, (col * CELL_SIZE, row * CELL_SIZE))  # Vẽ hình zombie vào vị trí tương ứng

def draw_timer(timer):
    timer_text = font.render(f"Time: {int(timer)}", True, BLACK)
    screen.blit(timer_text, (10, 10))

def draw_score(score, misses):
    score_text = font.render(f"Score: {score} Hits / {misses} Misses", True, BLACK)
    screen.blit(score_text, (10, 50))

def update_zombie(zombie_timer):
    global zombie_position, misses
    # Cập nhật vị trí zombie sau 2 giây
    if zombie_timer > 2:
        zombie_position = random.randint(0, ROWS * COLS - 1)
        misses += 1  # Tính là một lần miss nếu zombie tự thay đổi vị trí
        return 0  # Reset thời gian
    return zombie_timer

def button(text, x, y, w, h, inactive_color, active_color, action=None):
    mouse = pygame.mouse.get_pos()
    click = pygame.mouse.get_pressed()
    
    if x + w > mouse[0] > x and y + h > mouse[1] > y:
        pygame.draw.rect(screen, active_color, (x, y, w, h))
        if click[0] == 1 and action is not None:
            action()
    else:
        pygame.draw.rect(screen, inactive_color, (x, y, w, h))

    text_surf = font.render(text, True, WHITE)
    text_rect = text_surf.get_rect(center=((x + (w // 2)), (y + (h // 2))))
    screen.blit(text_surf, text_rect)

def game_intro():
    intro = True
    while intro:
        screen.fill(WHITE)
        screen.blit(background_image, (0, 0))  # Vẽ background

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                sys.exit()

        button("Start", 150, 350, 150, 50, GREEN, (0, 200, 0), game_loop)
        button("Quit", 500, 350, 150, 50, RED, (200, 0, 0), quit_game)

        pygame.display.flip()
        clock.tick(15)

def quit_game():
    pygame.quit()
    sys.exit()

def end_game():
    # Hiển thị kết quả cuối cùng và quay về màn hình chính
    screen.fill(WHITE)
    final_score_text = font.render(f"Final Score: {score} Hits / {misses} Misses", True, BLACK)
    screen.blit(final_score_text, (SCREEN_WIDTH // 4, SCREEN_HEIGHT // 2))
    pygame.display.flip()
    pygame.time.wait(3000)
    game_intro()  # Quay về màn hình chính

def game_loop():
    global zombie_position, score, misses

    # Khởi động lại giá trị ban đầu khi bắt đầu game
    zombie_position = random.randint(0, ROWS * COLS - 1)
    score = 0
    misses = 0
    timer = 30
    zombie_timer = 0

    while True:
        screen.blit(background_image, (0, 0))  # Vẽ background trước
        draw_grid()
        draw_zombie(zombie_position)
        draw_timer(timer)
        draw_score(score, misses)

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                sys.exit()
            elif event.type == pygame.MOUSEBUTTONDOWN:
                x, y = event.pos
                col = x // CELL_SIZE
                row = y // CELL_SIZE
                clicked_position = row * COLS + col

                if clicked_position == zombie_position:
                    score += 1
                    zombie_position = random.randint(0, ROWS * COLS - 1)
                    zombie_timer = 0  # Reset thời gian zombie khi đập trúng
                else:
                    misses += 1

        # Nút kết thúc game
        button("End", 650, 10, 100, 50, RED, (200, 0, 0), end_game)

        # Giảm thời gian và kết thúc trò chơi khi hết giờ
        if timer > 0:
            timer -= 1 / 30
            zombie_timer += 1 / 30  # Cập nhật thời gian zombie tồn tại
        else:
            end_game()  # Hiển thị kết quả và quay về màn hình chính

        zombie_timer = update_zombie(zombie_timer)  # Gọi hàm cập nhật vị trí zombie
        pygame.display.flip()
        clock.tick(30)

if __name__ == "__main__":
    game_intro()
