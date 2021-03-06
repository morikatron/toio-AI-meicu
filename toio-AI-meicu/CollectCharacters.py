import glob
from operator import truediv
from statistics import mode


scripts = glob.glob('Assets/meicu/Scripts/**/*.cs', recursive=True)
scenes = glob.glob('Assets/meicu/Scenes/*.unity')

print('=> Find {} scripts and {} scenes.'.format(len(scripts), len(scenes)))
print()

# sample = 'Assets/meicu/Scripts\\UI\\PageLearn.cs'

chas = set()

for script in scripts:
    with open(script, mode='r', encoding='utf-8') as file:
        while True:
            line = file.readline()
            if not line:
                break

            sublines = line.split('"')
            if len(sublines) < 2:
                continue

            strings = [sublines[i*2+1] for i in range(len(sublines)//2)]
            for string in strings:
                for cha in string:
                    chas.add(cha)


for scene in scenes:
    with open(scene, mode='r', encoding='utf-8') as file:
        while True:
            line = file.readline()
            if not line:
                break

            if line.find('m_Text: "') < 0 and line.find('m_text: "') < 0:
                continue

            string = line.split('"')[1]
            string = string.encode('utf-8').decode('unicode-escape')

            for cha in string:
                chas.add(cha)


# chas.remove('\u3000')
# chas.add('　')

# Sort
chas = list(chas)
chas.sort()

chas = ''.join(chas)
print('=> Find {} characters.'.format(len(chas)))
print(chas)

with open('charaters.txt', mode='w+', encoding='utf-8') as file:
    file.write(chas)

print('Written into characters.txt.')
print('Copy conetent to Unity TMP Font Asset Creator.')
