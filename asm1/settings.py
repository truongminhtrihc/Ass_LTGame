import pygame
import random
import sys
import os

# Khởi tạo Pygame
pygame.init()
# Tên file để lưu điểm cao nhất

HIGH_SCORE_FILE = os.path.join(os.path.dirname(__file__), 'high_score/highscore.txt')
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
