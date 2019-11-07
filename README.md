# Vasm Interpreter
Vasm is Virtual Assembly, op based language that I made up.
<br/>
## How its work

1) Open the project in visual studio/ vscode and compile it.
2) Create .vasm file and use the instractions/ops to write the program.
3) run the project where first argument is the file directory.
use flag -d to run in debug mode press 'W' to execute op.

## Interpreter Spec

R0 R1, two general purpose registers.
Ram size 512 bytes.
stack size 64 bytes.

## Instractions

M = Memory/Ram
R = Register  
N = Number  

inc - increase by one R0  
dec - decrease by one R0  
add M/R/N - add to R0  
sub M/R/N - subtract from R0  
mov M/R/N M/R - mov value from M/R/N to M/R   
jmp M/R/N - jump to place in memory  
je M/R/N - jump if R0 equels  
jne M/R/N - jump if R0 not equels  
jg M/R/N - jump if R0 greater  
jl M/R/N - jump if R0 lesser  
prt - print the number in R0  
psh - push R0 into the stack  
pop - pop number to R0  
