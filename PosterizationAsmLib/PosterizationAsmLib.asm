.data
    number_of_colors dd 255.0, 255.0, 255.0, 255.0
.code
posterize proc
    movaps xmm0, [number_of_colors]         ; filling xmm0 with 255

    ; filling xmm2 with (level-1)
    mov eax, r9d                            ; Load the value of r9d (width) into eax
    sub eax, 1                              ; Subtract 1 from eax (level-1)
    CVTSI2SS xmm2, eax                      ; Convert eax (level-1) to single-precision and store in xmm2
    shufps xmm2, xmm2, 0                    ; Replicate the value across xmm2



    ; calculating interval 1 ((level - 1)/255)
    movaps xmm3, xmm2                       ; moving xmm2(level-1) to xmm3
    divps xmm3, xmm0                        ; (level-1)/255
    

    ; calculating interval2 (255/(level - 1))
    divps xmm0,xmm2                         ; 255 / (level -1)

    ;rdx - width - start
    ; r8 - height - end

     ; Main loop for posterization
    mov     r13, rdx                        ; r13 = loop counter (filled with start parameter)

posterization_loop:
        ; xmm0 = interval2
        ; xmm3 = interval1

        ; filling xmm1 with bytes
        movzx eax, byte ptr[rcx + r13]      ; Load a byte with red channel to eax
        pinsrd xmm1, eax, 0                 ; Insert the byte into xmm1 at position 0
        movzx eax, byte ptr[rcx + r13 + 1]  ; Load a byte with green channel to eax
        pinsrd xmm1, eax, 1                 ; Insert the byte into xmm1 at position 1
        movzx eax, byte ptr[rcx + r13 + 2]  ; Load a byte with blue channel to eax
        pinsrd xmm1, eax, 2                 ; Insert the byte into xmm1 at position 2

        cvtdq2ps xmm1, xmm1                 ; Convert bytes into single-precision floating-points


        ;calculating result
        mulps xmm1, xmm3                    ; (pixel*interval1)
        roundps xmm1, xmm1, 0               ; rounding
        mulps xmm1, xmm0                    ; (pixel*interval1)*interval


        cvtps2dq xmm1, xmm1                 ; Convert single-precision floating-points into integers

        ; putting calculated channels back into memory
        pextrd eax, xmm1, 0                 ; Extract the first integer value from xmm1 to eax
        mov byte ptr[rcx + r13], al         ; Store the lower 8 bits of eax into the memory
        pextrd eax, xmm1, 1                 ; Extract the second integer value from xmm1 to eax
        mov byte ptr[rcx + r13 + 1], al     ; Store the lower 8 bits of eax into the memory at address [rcx + r13 + 1]
        pextrd eax, xmm1, 2                 ; Extract the third integer value from xmm1 to eax
        mov byte ptr[rcx + r13 + 2], al     ; Store the lower 8 bits of eax into the memory at address [rcx + r13 + 2]

        add     r13, 3                      ; increment loop counter by 4
        cmp     r13, r8                     ; check if it's the end   
        jl      posterization_loop          ; Continue loop if not done

ret
posterize endp
end 