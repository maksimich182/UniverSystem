CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('student', 'teacher', 'admin')),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS students (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID UNIQUE NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    group_name VARCHAR(20) NOT NULL,
    faculty VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS teachers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID UNIQUE NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    department VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS courses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    teacher_id UUID REFERENCES teachers(id),
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS grades (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id UUID NOT NULL REFERENCES students(id),
    course_id UUID NOT NULL REFERENCES courses(id),
    grade_value INTEGER NOT NULL CHECK (grade_value >= 1 AND grade_value <= 5),
    teacher_id UUID NOT NULL REFERENCES teachers(id),
    grade_date DATE NOT NULL DEFAULT CURRENT_DATE,
    created_at TIMESTAMP DEFAULT NOW()
);

INSERT INTO users (username, password_hash, email, role) VALUES 
('student1', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'student1@university.ru', 'student'),
('teacher1', '$2a$11$qS.kS2ozn/AM3/X2qDHDEu2msdE6L42kO9V4Ee8o8Cs5c5K8Zx8/O', 'teacher1@university.ru', 'teacher');

INSERT INTO students (user_id, first_name, last_name, group_name, faculty) VALUES 
('1', 'Евстигней', 'Абрикосов', 'ИТ-101', 'Информационные технологии');

INSERT INTO teachers (user_id, first_name, last_name, department) VALUES 
('2', 'Фома', 'Киняев', 'Кафедра информатики');

INSERT INTO courses (name, teacher_id) VALUES 
('Программирование', '44444444-4444-4444-4444-444444444444');
