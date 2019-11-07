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

$ - prefix for memory/ram location, for example $128 is 128 byte in memory.  
. - prefix for label and, example .multfive , can only use letter for the label.  
\# - comment, example # this is comment.

### Ops
M = Memory/Ram  
R = Register  
N = Number
L = Label

inc - increase by one R0  
dec - decrease by one R0  
add M/R/N - add to R0  
sub M/R/N - subtract from R0  
mov M/R/N M/R - mov value from M/R/N to M/R   
jmp M/R/N/L - jump to place in memory  
je M/R/N/L - jump if R0 equels  
jne M/R/N/L - jump if R0 not equels  
jg M/R/N/L - jump if R0 greater  
jl M/R/N/L - jump if R0 lesser  
prt - print the number in R0  
psh - push R0 into the stack  
pop - pop number to R0  

## Example

computes 5 to the power of 3

```
# $1 is the number, $2 is the power 
mov 3 $1 
mov 5 $2

mov $1 r0
dec
mov r0 $1

mov $2 $3
mov $2 $4

mov 0 $0

.exp
mov $1 r0
je end
dec
mov r0 $1

.mult
mov $3 r0
je endloop
dec
mov r0 $3
mov $0 r0
add $4
mov r0 $0
jmp mult

.endloop
mov $2 $3
mov $0 $4
mov 0  $0
jmp exp

.end
mov $4 r0
prt

```
