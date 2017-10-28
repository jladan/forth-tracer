\ Words for working with scene objects

\ Spheres
: svariable falign create 4 floats allot ;

: s! ( sphere -- ) ( f: f f f f -- )
    0 3 do
        dup I set-index 
    -1 +loop
    drop ;

: s@radius ( sphere -- ) ( f: -- f )
    3 get-index ;


\ Lights ( position and colour )
: lvariable falign create 6 floats allot ;

: lcolour ( light_addr -- colour_addr )
    3 floats + ;

: l! ( light -- ) ( f: x y z r g b -- )
    dup lcolour v!
    v! ;

: l@ ( light -- ) ( f: x y z r g b -- )
    dup v@ lcolour v@ ;
        
\ Materials ( colour, colour, shininess, mirror )
: mvariable falign create 8 floats allot ;

: mdiffuse ( material -- colour )
    0 floats + ;

: mspecular ( material -- colour )
    3 floats + ;

: mshininess ( material -- addr )
    6 floats + ;

: mmirror ( material -- addr )
    7 floats + ;

: m! ( material [8 floats] -- )
    dup mmirror f! 
    dup mshininess f! 
    dup mspecular v! 
    dup mdiffuse v!
    ;

\ extra stuff
: deg>rad 180e0 f/ 3.14159265e0 f* ;
