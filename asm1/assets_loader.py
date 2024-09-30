from settings import *

# Tải hình ảnh
background_image = pygame.image.load('assets/img/background.png')  # Tải hình ảnh background từ file PNG
background_image = pygame.transform.scale(background_image, (SCREEN_WIDTH, SCREEN_HEIGHT))  # Điều chỉnh kích thước background
zombie_image = pygame.image.load('assets/img/zombie3.png')  # Tải hình ảnh zombie từ file PNG
zombie_image = pygame.transform.scale(zombie_image, (CELL_SIZE-10, CELL_SIZE-10))  # Điều chỉnh kích thước zombie phù hợp với ô
tile_image = pygame.image.load('assets/img/tile2.png')  # Tải hình ảnh ô trống từ file PNG
tile_image = pygame.transform.scale(tile_image, (CELL_SIZE+30, CELL_SIZE+30))  # Điều chỉnh kích thước ô trống
hit_sound = pygame.mixer.Sound("assets/music/hit.wav")
miss_sound = pygame.mixer.Sound("assets/music/miss.wav")
gameover_sound = pygame.mixer.Sound("assets/music/gameover.wav")
gamerun_sound = pygame.mixer.Sound("assets/music/gamerun.wav")
clock = pygame.time.Clock()

hammer_image = pygame.image.load('assets/img/hammer.png')
hammer_image = pygame.transform.scale(hammer_image, (200, 200))  # Điều chỉnh kích thước cây búa nếu cần
hammer_hit_image = pygame.image.load('assets/img/hammer.png')
hammer_hit_image = pygame.transform.scale(hammer_hit_image, (250, 250))  # Điều chỉnh kích thước cây búa khi đập

# Tải hình ảnh stun
stun_image = pygame.image.load('assets/img/stunnedhit.png')
stun_image = pygame.transform.scale(stun_image, (CELL_SIZE + 20, CELL_SIZE + 20))  # Điều chỉnh kích thước stun
