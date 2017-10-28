\ scene geometry definition

vvariable cam_position
0e0 0e0 800e0 cam_position v!
vvariable cam_direction
0e0 0e0 -1e0 cam_direction v!

vvariable ver
0e0 1e0 0e0 ver v!
vvariable hor
cam_direction ver v-cross hor v!

50e0 deg>rad fconstant fov

vvariable ambient
0.3e0 0.3e0 0.3e0 ambient v!


\ all the spheres
create spheres 5 cells allot
falign
here 0 floats +  spheres 0 cells + !
here 4 floats +  spheres 1 cells + !
here 8 floats +  spheres 2 cells + !
here 12 floats +  spheres 3 cells + !
here 16 floats +  spheres 4 cells + !
4 floats 5 * allot

\ lights
create lights 2 cells allot
falign
here 0 floats +  lights 0 cells + !
here 6 floats +  lights 1 cells + !
6 floats 2 * allot

\ materials
create mats 5 cells allot
falign
here 0 floats +  mats 0 cells + !
here 8 floats +  mats 1 cells + !
here 16 floats +  mats 2 cells + !
here 24 floats +  mats 3 cells + !
here 32 floats +  mats 4 cells + !
8 floats 5 * allot



\ Most of the actual tracer


\ The tracer settings

512 constant width
512 constant height

width s>f height s>f f/ fconstant aspect_ratio

height s>f   fov 2e0 f/ ftan f/  2e0 f/
fconstant view_len


vvariable top_pixel
vvariable ray
vvariable pixel
vvariable vtmp

vvariable colour

1e-6 fconstant epsilon
            
variable file-handle

: write-ppm-header
    s" traced.ppm" w/o create-file 
    0= if  file-handle ! then
    s" P6" file-handle @ write-line drop
    width s>d <# #s #> file-handle @ write-file drop
    32 file-handle @ emit-file drop
    height s>d <# #s #> file-handle @ write-line drop
    s" 255" file-handle @ write-line drop
    ;

: p-emit ( colour -- )
    v@
    1e0 fmin 255e0 f* f>s 
    1e0 fmin 255e0 f* f>s
    1e0 fmin 255e0 f* f>s
    file-handle @ emit-file drop
    file-handle @ emit-file drop
    file-handle @ emit-file drop
    ;

\ set the value of top_pixel
: initialize-tracer ( -- )
    0e0 0e0 -400e0 100e0      spheres 0 cells + @ s!
    200e0 50e0 -100e0 150e0   spheres 1 cells + @ s!
    0e0 -1200e0 -500e0 1000e0 spheres 2 cells + @ s!
    -100e0 25e0 -300e0 50e0   spheres 3 cells + @ s!
    0e0 100e0 -250e0 25e0     spheres 4 cells + @ s!

    -100e0 150e0 400e0 7e-1 7e-1 7e-1 lights 0 cells + @ l!
    400e0  100e0 150e0 7e-1 0e0  7e-1 lights 1 cells + @ l!

    7e-1 1e0 7e-1 5e-1 7e-1 5e-1 25e0 3e-1 mats 0 cells + @ m!
    7e-1 1e0 7e-1 5e-1 7e-1 5e-1 25e0 3e-1 mats 1 cells + @ m!
    5e-1 5e-1 5e-1 5e-1 7e-1 5e-1 25e0 3e-1 mats 2 cells + @ m!
    1e0 6e-1 1e-1 5e-1 7e-1 5e-1 25e0 3e-1 mats 3 cells + @ m!
    7e-1 1e0 7e-1 5e-1 7e-1 5e-1 25e0 3e-1 mats 4 cells + @ m!
    
    cam_direction view_len vf* top_pixel v!
    top_pixel cam_position v+=
    ver height s>f f2/ vf* vtmp v!
    top_pixel vtmp v+=
    hor width s>f f2/ vf* vtmp v!
    top_pixel vtmp v-=
    ;

: ray-epsilon-check { normal ray line f: raylen -- flag }
    raylen epsilon f> if
        ray raylen vf* normal v!    \ ray multiplied by root and put in normal
        normal line v-=             \ line subtracted from result and put in normal
        raylen
        -1                       \ it worked!
    else
        0                        \ it didn't work :(
    then ;

: quad-roots ( -- numroots ) { f: a f: b f: c -- [f] [f] }
    a f0= if
        c b f/ fneg     \ -c/b
        1               \ one root
    else
        b fdup f*
        a c f2* f2* f* f-                   ( f: det )
        fdup f0< 0= if
            fsqrt
            b fneg fover f+                 ( f: det -b+det )
            a f/ f2/ fswap                  ( f: root1 det )
            b fneg fswap f-                 ( f: det -b-det )
            a f/ f2/                        ( f: root1 root2 )
            2
        else
            fdrop
            0
        then
    then ;

vvariable line

: intersect-sphere { ray origin sphere normal -- raylen flag }
    sphere origin v- line v!
    ray ray v-dot
    ray line v-dot fneg f2*
    line line v-dot sphere s@radius fdup f* f-

    quad-roots
    case 
        0 of 0 endof
        1 of normal ray line ray-epsilon-check endof
        2 of
            fover fover f< if
                fdrop
                normal ray line ray-epsilon-check
            else
                fswap fdrop
                normal ray line ray-epsilon-check
            then
        endof
    endcase ;

: intersect-spheres { ray origin normal -- sphere f: raylen flag }
    -1 1e10              \ preload sphere = -1, raylen = 10^10
    5 0 do 
        ray origin spheres i cells + @ vtmp intersect-sphere
        if
            fover fover f> if       \ new raylen is smaller
                vtmp v@ normal v!
                drop i              \ set sphere to current sphere
                fswap fdrop         \ replace old raylen with new one
            endif
        endif 
    loop 
    dup -1 = if         \ we didn't find a sphere
        fdrop drop      \ clear stacks
        0               \ no intersection
    else
        -1              \ set flag to true
    endif ;

vvariable diffuse
vvariable specular
vvariable shadow_ray
vvariable normal
vvariable intersection

: trace { colour ray origin -- }
    ray origin normal intersect-spheres
    if
        0e0 0e0 0e0 diffuse v!
        0e0 0e0 0e0 specular v!
        ray vf* intersection v!
        intersection origin v+=
        normal v-norm
        ray v-norm

        2 0 do
            lights i cells + @ intersection v- shadow_ray v!
            shadow_ray intersection vtmp intersect-spheres
            if 
                fdrop drop \ we don't do anything if there's an intersection
            else
                shadow_ray v-norm
                normal shadow_ray v-dot
                fdup
                fdup f0> if
                    dup cells mats + @ mdiffuse vf* vtmp v!
                    vtmp lights i cells + @ lcolour v* vtmp v!
                    diffuse vtmp v+=
                else
                    fdrop
                endif
                normal f2* vf* vtmp v!
                shadow_ray vtmp v-=
                shadow_ray ray v-dot
                dup cells mats + @ mshininess f@ f**
                fdup f0> if
                    dup cells mats + @ mspecular vf* vtmp v!
                    i cells lights + @ lcolour vtmp v* vtmp v!
                    specular vtmp v+= 
                else
                    fdrop       \ drop bad specular coefficient
                endif
            endif
        loop
        cells mats + @ ambient v* colour v!
        colour diffuse v+=
        colour specular v+=
    else
        0e0 0e0 0e0 colour v!
    endif ;


: ii R> R> R> R> R> dup >R >R >R >R >R ;
: jj R> R> R> R> R> R> R> dup >R >R >R >R >R >R >R ;

vvariable sub-pixel
vvariable tmp_colour

4 constant naa

: trace-loop ( -- )
    height 0 do
        width 0 do
            0e0 0e0 0e0 colour v!
            \ find location of top pixel
            hor i s>f vf* pixel v!
            ver j s>f fneg vf* vtmp v!
            pixel vtmp v+=
            pixel top_pixel v+=
            \ anti-aliasing
            naa 0 do
            naa 0 do
                hor i s>f naa s>f f/ vf* sub-pixel v!
                ver j s>f naa s>f f/ vf* vtmp v!
                sub-pixel vtmp v+=
                sub-pixel pixel v+=
                sub-pixel cam_position v- ray v!
                pixel cam_position v- ray v!
                tmp_colour ray cam_position trace
                colour tmp_colour v+=
            loop
            loop
            naa dup * s>f colour vf/ colour v!
            colour p-emit
        loop
    loop ;

 
initialize-tracer
write-ppm-header
trace-loop
file-handle @ close-file

bye
