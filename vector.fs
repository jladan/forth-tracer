\ Vector arithmetic library

: fneg
    0e0 fswap f- ;

\ makes a 3-vector
\ eg. vvariable x
: vvariable falign create 3 floats allot ;

\ now for array indexing
: get-index ( vec n --) ( f: -- f)
    floats + f@ ;

: set-index ( vec n --) ( f: f --)
    floats + f! ;

: v@ ( vec --) ( f: -- f f f)
    3 0 do dup I get-index loop drop ;

: v! ( vec --) ( f: f f f --)
    0 2 do dup I set-index -1 +loop drop ;

\ vector arithmetic
: v+ ( vec vec --) ( f: -- f f f)
    3 0 
    do over over I get-index I get-index f+ 
    loop
    drop drop ;

: v+= ( vec vec -- ) ( f: -- )
    over v+ v! ;

: v- ( vec vec --) ( f: -- f f f)
    swap
    3 0 
    do 2dup I get-index I get-index f- 
    loop
    drop drop ;

: v-= ( vec vec -- ) ( f: -- )
    over swap v- v! ;

: vf* ( vec -- ) ( f: f -- f f f)
    3 0
    do fdup dup I get-index f* fswap
    loop
    fdrop drop ;

: vf/ ( vec -- f f f )
    1e0 fswap f/ vf* ;

: v* ( vec vec --) ( f: -- f f f)
    3 0 
    do over over I get-index I get-index f* 
    loop
    drop drop ;

\ dot and cross products
: v-dot ( vec vec -- ) ( f: -- f)
    v* f+ f+ ;

: v-cross ( vec vec --) ( f: -- f f f)
    swap
    2dup 2dup
    1 get-index 2 get-index f*
    2 get-index 1 get-index f*
    f-
    2dup 2dup
    2 get-index 0 get-index f*
    0 get-index 2 get-index f*
    f-
    2dup
    0 get-index 1 get-index f*
    1 get-index 0 get-index f*
    f-
    ;

: v-norm ( vec -- )
    dup dup v-dot fsqrt
    dup vf/
    v! ;

\ display vectors
: 3f. ( f: f f f --)
    fswap frot f. f. f. ;
    
: v. ( vec -- ) 
    v@ 3f. ;

