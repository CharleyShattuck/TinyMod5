\ main.fs
target

\ Arduino constants
0 wconstant INPUT
1 wconstant OUTPUT
2 wconstant INPUT_PULLUP

\ serial mode's array 
variable data 4 ramALLOT \ 6 bytes in all
: /data  data a! 5 #, for false c!+ next ; 

: initPortExpander  $20 #, initMCP23017 ;
: initPins
    INPUT_PULLUP  5 #, pinMode
    INPUT_PULLUP  7 #, pinMode
    INPUT_PULLUP  9 #, pinMode
    INPUT_PULLUP 10 #, pinMode
    INPUT_PULLUP 11 #, pinMode
    INPUT_PULLUP 12 #, pinMode
    INPUT_PULLUP 18 #, pinMode
    INPUT_PULLUP 19 #, pinMode
    INPUT_PULLUP 20 #, pinMode
    INPUT_PULLUP 21 #, pinMode
    INPUT_PULLUP 22 #, pinMode
    INPUT_PULLUP 23 #, pinMode
    ;
: init  initPortExpander initPins ;

\ read the keyboard
: pin| ( n mask pin - n')  @pin and or ;
: @pins (  - n)
    $20 #, @MCP23017  0 #, 
     $010000 #,  9 #, pin|
     $020000 #, 10 #, pin|
     $040000 #, 11 #, pin|
     $080000 #, 12 #, pin|
     $100000 #, 19 #, pin|
     $200000 #, 20 #, pin|
     $400000 #, 21 #, pin|
     $800000 #, 22 #, pin|
    $1000000 #, 23 #, pin|
    or $1ff0000 #, xor ;

\ get one stroke
: press (  - n)  false begin drop @pins until ;
: release ( n1 - n2)  begin @pins while or repeat drop ;
: scan (  - n)
    begin press 30 #, ms @pins if or release exit then drop drop again

\ vectored emit, for testing
wvariable 'spit  \ execution tokens are 16 bits
: spit  'spit w@ execute ; 
: >emit  ['] emit 'spit w! ; 
: >hc.  ['] hc. 'spit w! ; 
: send  data a! 5 #, for c@+ spit next ; \ Gemini
: ?send  data a!  \ TX Bolt
    c@+ if dup spit then drop
    c@+ if dup $40 #, or spit then drop
    c@+ if dup $80 #, or spit then drop
    c@+ if $c0 #, or spit exit then spit ;

\ Gemini protocol to the data array
: mark ( mask a)  data + dup >r c@ or r> c! ; 
: Gemini ( n)  /data $80 #, data c!
    dup $0100000 #, and if/ $40 #, 1 #, mark then \ S1
    dup $0200000 #, and if/ $10 #, 1 #, mark then \ T
    dup $0400000 #, and if/ $04 #, 1 #, mark then \ P
    dup $0800000 #, and if/ $01 #, 1 #, mark then \ H
    dup $1000000 #, and if/ $08 #, 2 #, mark then \ *
    dup $0008000 #, and if/ $02 #, 3 #, mark then \ F
    dup $0004000 #, and if/ $40 #, 4 #, mark then \ P
    dup $0002000 #, and if/ $10 #, 4 #, mark then \ L
    dup $0001000 #, and if/ $04 #, 4 #, mark then \ T
    dup $0000100 #, and if/ $01 #, 4 #, mark then \ D
    dup $0080000 #, and if/ $20 #, 1 #, mark then \ S2
    dup $0040000 #, and if/ $08 #, 1 #, mark then \ K
    dup $0020000 #, and if/ $02 #, 1 #, mark then \ W
    dup $0010000 #, and if/ $40 #, 2 #, mark then \ R
    dup $0000200 #, and if/ $04 #, 2 #, mark then \ *
    dup $0000001 #, and if/ $01 #, 3 #, mark then \ R
    dup $0000002 #, and if/ $20 #, 4 #, mark then \ B
    dup $0000004 #, and if/ $08 #, 4 #, mark then \ G
    dup $0000800 #, and if/ $02 #, 4 #, mark then \ S
    dup $0000400 #, and if/ $01 #, 5 #, mark then \ Z
    dup $0000008 #, and if/ $20 #, 2 #, mark then \ A
    dup $0000010 #, and if/ $10 #, 2 #, mark then \ O 
    dup $0000020 #, and if/ $40 #, 5 #, mark then \ #
    dup $0000040 #, and if/ $08 #, 3 #, mark then \ E
    dup $0000080 #, and if/ $04 #, 3 #, mark then \ U
    drop ; 
: send-Gemini  Gemini send ;

\ TX Bolt protocol to the data array
: bolt ( n)  /data
    dup $0100000 #, and if/ $01 #, 0 #, mark then \ S1
    dup $0080000 #, and if/ $01 #, 0 #, mark then \ S2
    dup $0200000 #, and if/ $02 #, 0 #, mark then \ T
    dup $0040000 #, and if/ $04 #, 0 #, mark then \ K
    dup $0400000 #, and if/ $08 #, 0 #, mark then \ P
    dup $0020000 #, and if/ $10 #, 0 #, mark then \ W
    dup $0800000 #, and if/ $20 #, 0 #, mark then \ H
    dup $0010000 #, and if/ $01 #, 1 #, mark then \ R
    dup $0000008 #, and if/ $02 #, 1 #, mark then \ A
    dup $0000010 #, and if/ $04 #, 1 #, mark then \ O 
    dup $1000000 #, and if/ $08 #, 1 #, mark then \ *
    dup $0000200 #, and if/ $08 #, 1 #, mark then \ *
    dup $0000040 #, and if/ $10 #, 1 #, mark then \ E
    dup $0000080 #, and if/ $20 #, 1 #, mark then \ U
    dup $0008000 #, and if/ $01 #, 2 #, mark then \ F
    dup $0000001 #, and if/ $02 #, 2 #, mark then \ R
    dup $0004000 #, and if/ $04 #, 2 #, mark then \ P
    dup $0000002 #, and if/ $08 #, 2 #, mark then \ B
    dup $0002000 #, and if/ $10 #, 2 #, mark then \ L
    dup $0000004 #, and if/ $20 #, 2 #, mark then \ G
    dup $0001000 #, and if/ $01 #, 3 #, mark then \ T
    dup $0000800 #, and if/ $02 #, 3 #, mark then \ S
    dup $0000100 #, and if/ $04 #, 3 #, mark then \ D
    dup $0000400 #, and if/ $08 #, 3 #, mark then \ Z
    dup $0000020 #, and if/ $10 #, 3 #, mark then \ #
    drop ; 
: send-TXBolt  bolt ?send ;

\ A-Z
: spout ( stroke char mask - stroke )
    2 #, ms
    rot dup >r and if/ Keyboard.write r> exit then
    drop BL Keyboard.write r> ;
: send-A-Z ( stroke) 
    [ char # ] #, $0000020 #, spout
    [ char S ] #, $0180000 #, spout
    [ char T ] #, $0200000 #, spout
    [ char K ] #, $0040000 #, spout
    [ char P ] #, $0400000 #, spout
    [ char W ] #, $0020000 #, spout
    [ char H ] #, $0800000 #, spout
    [ char R ] #, $0010000 #, spout
    [ char A ] #, $0000008 #, spout
    [ char O ] #, $0000010 #, spout
    [ char * ] #, $1000200 #, spout
    [ char E ] #, $0000040 #, spout
    [ char U ] #, $0000080 #, spout
    [ char F ] #, $0008000 #, spout
    [ char R ] #, $0000001 #, spout
    [ char P ] #, $0004000 #, spout
    [ char B ] #, $0000002 #, spout
    [ char L ] #, $0002000 #, spout
    [ char G ] #, $0000004 #, spout
    [ char T ] #, $0001000 #, spout
    [ char S ] #, $0000800 #, spout
    [ char D ] #, $0000100 #, spout
    [ char Z ] #, $0000400 #, spout
    5 #, ms 10 #, Keyboard.write
    5 #, ms 13 #, Keyboard.write
    drop ;

\ NKRO keyboard mode
cvariable former
: spew ( c - )
    dup Keyboard.press
    former c@ if dup Keyboard.release then
    drop former c! ; 
: send-NKRO ( n - )
    false former c!
    dup  $100000 #, and if/ [ char q ] #, spew then
    dup  $200000 #, and if/ [ char w ] #, spew then
    dup  $400000 #, and if/ [ char e ] #, spew then
    dup  $800000 #, and if/ [ char r ] #, spew then
    dup $1000000 #, and if/ [ char t ] #, spew then
    dup $8000 #, and if/ [ char u ] #, spew then
    dup $4000 #, and if/ [ char i ] #, spew then
    dup $2000 #, and if/ [ char o ] #, spew then
    dup $1000 #, and if/ [ char p ] #, spew then
    dup  $100 #, and if/ [ char [ ] #, spew then
    dup $80000 #, and if/ [ char a ] #, spew then
    dup $40000 #, and if/ [ char s ] #, spew then
    dup $20000 #, and if/ [ char d ] #, spew then
    dup $10000 #, and if/ [ char f ] #, spew then
    dup   $200 #, and if/ [ char g ] #, spew then
    dup  $01 #, and if/ [ char j ] #, spew then
    dup  $02 #, and if/ [ char k ] #, spew then
    dup  $04 #, and if/ [ char l ] #, spew then
    dup $800 #, and if/ [ char ; ] #, spew then
    dup $400 #, and if/ [ char ' ] #, spew then
    dup $08 #, and if/ [ char c ] #, spew then
    dup $10 #, and if/ [ char v ] #, spew then
    dup $20 #, and if/ [ char 3 ] #, spew then
    dup $40 #, and if/ [ char n ] #, spew then
    dup $80 #, and if/ [ char m ] #, spew then
    drop Keyboard.releaseAll ; 

\ slider switch determines the protocol
: @sliders ( - n)
    5 #, @pin invert 4 #, and
    18 #, @pin invert 2 #, and or
    7 #, @pin invert 1 #, and or ;

create protocols
    ', send-NKRO
    ', send-Gemini
    ', send-TXBolt
    ', send-A-Z
    ', send-NKRO    \ these are
    ', send-Gemini  \ free for
    ', send-TXBolt  \ future expansion
    ', abort  \ this runs interpret

turnkey decimal init Keyboard.begin >hc.
    begin @sliders 7 #, = while/ interpret
    repeat >emit
    begin  scan @sliders protocols + @p execute
    again

