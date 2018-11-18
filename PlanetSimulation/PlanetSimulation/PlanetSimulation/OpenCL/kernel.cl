void PrintFloat2(float2 vec)
{
	printf("%.10f, %.10f\n", vec.x, vec.y);
}

void Gravity(
	__global float2 * out_dir1, 
	__global float2 * out_dir2,
	__constant float2 * pos1, float mass1,
	__constant float2 * pos2, float mass2,
	float simSpeed, float elapsedTime)
{
	float2 directionVec = *pos2 - *pos1;

	float dis = length(directionVec);
	if(!dis)
		return;
		
	dis = dis * dis;
	
	float acceleration = 0.0000000000000000667408F * simSpeed / dis * elapsedTime;
	
	directionVec = normalize(directionVec) * acceleration;

	(*out_dir1) += (directionVec * mass2);
	(*out_dir2) -= (directionVec * mass1);
}


__kernel void CalculateGravity(
	__global float * output,
	__constant float * input,
	float simSpeed, float elapsedTime,
	int index1, int index2) 
{
	__constant float2 * pos1 = (__constant float2 *)(input + 3 * index1);
	__constant float2 * pos2 = (__constant float2 *)(input + 3 * index2);
	__global float2 * dir1 = (__global float2 *)(output + 2 * index1);
	__global float2 * dir2 = (__global float2 *)(output + 2 * index2);
	__constant float * mass1 = (__constant float2 *)(input + 3 * index1 + 2);
	__constant float * mass2 = (__constant float2 *)(input + 3 * index2 + 2);

	//PrintFloat2(*pos1);
	//printf("%.10f", *mass1);

	Gravity(dir1, dir2, pos1, *mass1, pos2, *mass2, simSpeed, elapsedTime);
	
	
	/*float2 dir1Vec;
	dir1Vec.x = out_dir1[0];
	dir1Vec.y = out_dir1[1];
	
	float2 dir2Vec;
	dir2Vec.x = out_dir2[0];
	dir2Vec.y = out_dir2[1];*/

	/*PrintFloat2(pos1Vec, "pos1");
	PrintFloat2(pos2Vec, "pos2");
	PrintFloat2(dir1Vec, "dir1");
	PrintFloat2(dir2Vec, "dir2");
	printf("mass1 - %f \n", mass1);
	printf("mass2 - %f \n", mass2);
	printf("simSpeed - %f \n", simSpeed);
	printf("elapsedTime - %f \n", elapsedTime);*/
}