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

INSERT INTO users (id, username, password_hash, email, role) VALUES 
('33333333-3333-3333-3333-333333333333', 'student2', '$2a$11$/g8ScnDX7Jzc/A.xlZAhw.Iv.MpyjYZ3qdJB9diNOH0G1ebZluQBC', 'student2@university.ru', 'student'),
('44444444-4444-4444-4444-444444444444', 'student3', '$2a$11$Aw/YComNB1RJovYN3xBZi.uj8S1swNBFoPtJCJ9z0o09EMilElc5y', 'student3@university.ru', 'student'),
('55555555-5555-5555-5555-555555555555', 'student4', '$2a$11$Ujy2DJX948jWQP6V3jOv0.9MdEppei3FJ.06hmwnRSD.jwtqFc59q', 'student4@university.ru', 'student'),
('66666666-6666-6666-6666-666666666666', 'teacher2', '$2a$11$PV3z7h54y1dIBrCF3KfGN.xN0dtk7vxMRFfVXESCf.4dA8qf8yUE6', 'teacher2@university.ru', 'teacher'),
('77777777-7777-7777-7777-777777777777', 'teacher3', '$2a$11$FEkJ3bbe.k193632jcM8JuvgTwedAVboUnY6.AHTlgrlUcsChjw7.', 'teacher3@university.ru', 'teacher'),
('88888888-8888-8888-8888-888888888888', 'admin1', '$2a$11$3SkER7VyBqQEiCMgpBBCsuU31iK0vGWytDyiLPpLdAGX/jLyZoFSG', 'admin@university.ru', 'admin');

-- Дополняем студентов
INSERT INTO students (id, user_id, first_name, last_name, group_name, faculty) VALUES 
('66666666-6666-6666-6666-666666666666', '33333333-3333-3333-3333-333333333333', 'Анна', 'Петрова', 'ИТ-101', 'Информационные технологии'),
('77777777-7777-7777-7777-777777777777', '44444444-4444-4444-4444-444444444444', 'Сергей', 'Сидоров', 'ИТ-102', 'Информационные технологии'),
('88888888-8888-8888-8888-888888888888', '55555555-5555-5555-5555-555555555555', 'Мария', 'Козлова', 'ИТ-101', 'Информационные технологии'),
('99999999-9999-9999-9999-999999999999', '88888888-8888-8888-8888-888888888888', 'Админ', 'Админов', 'АДМ-001', 'Администрация');

-- Дополняем преподавателей
INSERT INTO teachers (id, user_id, first_name, last_name, department) VALUES 
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '66666666-6666-6666-6666-666666666666', 'Мария', 'Иванова', 'Кафедра программирования'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '77777777-7777-7777-7777-777777777777', 'Алексей', 'Смирнов', 'Кафедра алгоритмов'),
('cccccccc-cccc-cccc-cccc-cccccccccccc', '88888888-8888-8888-8888-888888888888', 'Админ', 'Админов', 'Администрация');

-- Дополняем курсы
INSERT INTO courses (id, name, teacher_id) VALUES 
('66666666-6666-6666-6666-666666666666', 'Базы данных', '44444444-4444-4444-4444-444444444444'),
('77777777-7777-7777-7777-777777777777', 'Алгоритмы и структуры данных', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'),
('88888888-8888-8888-8888-888888888888', 'Веб-разработка', '44444444-4444-4444-4444-444444444444'),
('99999999-9999-9999-9999-999999999999', 'Математический анализ', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Операционные системы', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Компьютерные сети', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb');

-- Добавляем оценки для тестирования
INSERT INTO grades (id, student_id, course_id, grade_value, teacher_id, grade_date) VALUES 
-- Оценки для student1 (Евстигней Абрикосов)
('cccccccc-cccc-cccc-cccc-cccccccccccc', '33333333-3333-3333-3333-333333333333', '55555555-5555-5555-5555-555555555555', 5, '44444444-4444-4444-4444-444444444444', '2024-01-15'),
('dddddddd-dddd-dddd-dddd-dddddddddddd', '33333333-3333-3333-3333-333333333333', '66666666-6666-6666-6666-666666666666', 4, '44444444-4444-4444-4444-444444444444', '2024-01-20'),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '33333333-3333-3333-3333-333333333333', '77777777-7777-7777-7777-777777777777', 5, 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '2024-01-25'),

-- Оценки для student2 (Анна Петрова)
('ffffffff-ffff-ffff-ffff-ffffffffffff', '66666666-6666-6666-6666-666666666666', '55555555-5555-5555-5555-555555555555', 4, '44444444-4444-4444-4444-444444444444', '2024-01-15'),
('11111111-1111-1111-1111-111111111112', '66666666-6666-6666-6666-666666666666', '88888888-8888-8888-8888-888888888888', 3, '44444444-4444-4444-4444-444444444444', '2024-01-22'),
('22222222-2222-2222-2222-222222222223', '66666666-6666-6666-6666-666666666666', '99999999-9999-9999-9999-999999999999', 5, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '2024-01-28'),

-- Оценки для student3 (Сергей Сидоров)
('33333333-3333-3333-3333-333333333334', '77777777-7777-7777-7777-777777777777', '55555555-5555-5555-5555-555555555555', 3, '44444444-4444-4444-4444-444444444444', '2024-01-15'),
('44444444-4444-4444-4444-444444444445', '77777777-7777-7777-7777-777777777777', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 4, 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '2024-01-26'),
('55555555-5555-5555-5555-555555555556', '77777777-7777-7777-7777-777777777777', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 5, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '2024-01-30'),

-- Оценки для student4 (Мария Козлова)
('66666666-6666-6666-6666-666666666667', '88888888-8888-8888-8888-888888888888', '66666666-6666-6666-6666-666666666666', 5, '44444444-4444-4444-4444-444444444444', '2024-01-20'),
('77777777-7777-7777-7777-777777777778', '88888888-8888-8888-8888-888888888888', '77777777-7777-7777-7777-777777777777', 4, 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '2024-01-25'),
('88888888-8888-8888-8888-888888888889', '88888888-8888-8888-8888-888888888888', '99999999-9999-9999-9999-999999999999', 5, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '2024-01-28');
