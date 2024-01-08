
.code
posterize proc
    mov r10, rdx    ; move width from rdx register


    ; filling xmm0 with 255
    mov     eax, 255         
    CVTSI2SS xmm0, eax       

    punpckldq xmm0, xmm0
    punpcklqdq xmm0, xmm0


    ; filling xmm2 with (level-1)
    mov eax, r9d
    sub eax, 1

    CVTSI2SS xmm2, eax       

    punpckldq xmm2, xmm2
    punpcklqdq xmm2, xmm2




    ; Calculate the total number of pixels
    imul    r10, r8            ; r10 = width * height
    imul    r10, 4

     ; Main loop for posterization
    xor     r13, r13            ; r13 = loop counter


posterization_loop:

        ; filling xmm1 with bytes
        movzx eax, byte ptr[rcx + r13]
        pinsrd xmm1, eax, 0
        movzx eax, byte ptr[rcx + r13 + 1]
        pinsrd xmm1, eax, 1
        movzx eax, byte ptr[rcx + r13 + 2]
        pinsrd xmm1, eax, 2
        movzx eax, byte ptr[rcx + r13 + 3]
        pinsrd xmm1, eax, 3

        ; converting bytes into single-precision floating-points
        cvtdq2ps xmm1, xmm1


        divps xmm1, xmm0         ; /255
        mulps xmm1, xmm2         ; *(level-1)
        roundps xmm1, xmm1, 0   ; round
        divps xmm0, xmm2         ; 255/(level-1)
        mulps xmm1, xmm0



        cvtps2dq xmm1, xmm1      ; converting single-precision floating-points into integers

        ; extracting calculated pixel and putting it back into bitmap
        pextrd eax, xmm1, 0
        mov byte ptr[rcx + r13], al
        pextrd eax, xmm1, 1
		mov byte ptr[rcx + r13 + 1], al
        pextrd eax, xmm1, 2
		mov byte ptr[rcx + r13 + 2], al

        add     r13, 4  
        cmp     r13, r10
        jl      posterization_loop   ; Continue loop if not done

ret
posterize endp
end

