__kernel void double_kernel(__global float* input1, __global float* input2, __global float* output)
{
    int i = get_global_id(0); 
    float f1 = input1[i]-input2[i];
    float f2 = input1[i+1]-input2[i+1];
    float f3 = input1[i+2]-input2[i+2];

    float r1 = pow(f1,2);  
    float r2 = pow(f2,2);         
    float r3 = pow(f3,2);         
    float r = r1+r2+r3;
    output[i] = sqrt(r); 
}