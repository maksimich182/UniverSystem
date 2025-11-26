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

INSERT INTO users (id, username, password_hash, email, role) VALUES 
('11111111-1111-1111-1111-111111111111', 'student1', '$2a$11$rfSGzVYqENm6PS5/xQfXZOUd1fqsLC0ZZF/7R4n2zMZpKvCJQVO5G', 'student1@university.ru', 'student'),
('22222222-2222-2222-2222-222222222222', 'teacher1', '$2a$11$.AIMXqmHk6crfIKPt.wPJ.J.aePADnYooSJXRA4iYHD.ohbVzwzOK', 'teacher1@university.ru', 'teacher');

INSERT INTO students (id, user_id, first_name, last_name, group_name, faculty) VALUES 
('33333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111', 'Евстигней', 'Абрикосов', 'ИТ-101', 'Информационные технологии');

INSERT INTO teachers (id, user_id, first_name, last_name, department) VALUES 
('44444444-4444-4444-4444-444444444444', '22222222-2222-2222-2222-222222222222', 'Фома', 'Киняев', 'Кафедра информатики');

INSERT INTO courses (id, name, teacher_id) VALUES 
('55555555-5555-5555-5555-555555555555', 'Программирование', '44444444-4444-4444-4444-444444444444');

