import os

# Укажите имя выходного файла
output_file = 'Plug.txt'

# Определяем корневую директорию проекта как директорию, в которой находится текущий скрипт
project_root = os.path.dirname(os.path.abspath(__file__))

# Открываем выходной файл для записи
with open(output_file, 'w', encoding='utf-8') as outfile:
    # Рекурсивно обходим все файлы в проекте
    for root, dirs, files in os.walk(project_root):
        for file in files:
            if file.endswith('.cs'):
                file_path = os.path.join(root, file)
                try:
                    # Открываем текущий файл для чтения
                    with open(file_path, 'r', encoding='utf-8') as infile:
                        # Записываем путь к файлу
                        outfile.write(f'--- {file_path} ---\n')
                        # Записываем содержимое файла
                        outfile.write(infile.read())
                        outfile.write('\n\n')
                except Exception as e:
                    # Если не удается прочитать файл, записываем сообщение об ошибке
                    outfile.write(f'--- {file_path} ---\n')
                    outfile.write(f'Error reading file: {e}\n\n')

print(f'Проект успешно объединен в файл {output_file}')
