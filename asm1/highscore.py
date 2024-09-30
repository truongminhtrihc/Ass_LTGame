from settings import *
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
