@echo off
echo zip,dll,exe�t�@�C����Zone���ʎq���폜���܂��B

echo on

DIR /B /S > dir.txt
FOR /f %%a in (dir.txt) do (
echo . > %%a:Zone.Identifier
)
DEL dir.txt

@echo off
echo �������܂����B
echo on
