
import pygame
import random
import sys
import os
from settings import *
from assets_loader import *
from highscore import *
from zombie import *
from ui import *
from game import *

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
        
        # Hiển thị stun với fade-in
        if stun_active:
            if stun_alpha < 255:
                stun_alpha += 35  # Tăng dần giá trị alpha để tạo hiệu ứng fade-in
            draw_stun(last_zombie_position, stun_alpha)

        button("End", 650, 10, 100, 50, RED, (200, 0, 0), end_game)
        
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
