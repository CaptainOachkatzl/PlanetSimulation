// some global RRT array
__global int * _rrtMatrix;
int _steps;
int _cores;

void SetRRTMatrix(
	__global int * in_matrix,
	int steps,
	int cores)
{
	_rrtMatrx = in_matrx;
	_steps = steps;
	_cores = cores;
}

__global float2 * _pos;
__global float2 * _dir;
__global float * _mass;
int _planetCount;
float _elapsedTime;
float _simSpeed;

void SetPlanetData(
	__global float2 * pos,
	__global float2 * dir,
	__global float * mass,
	int planetCount,
	float elapsedTime,
	float simSpeed)
{
	_pos = pos;
	_dir = dir;
	_mass = mass;
	_planetCount = planetCount;
	_elapsedTime = elapsedTime;
	_simSpeed = simSpeed;
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
	if (!dis)
		return;

	dis = dis * dis;

	float acceleration = 0.0000000000000000667408F * simSpeed / dis * elapsedTime;

	directionVec = normalize(directionVec) * acceleration;

	(*out_dir1) += (directionVec * mass2);
	(*out_dir2) -= (directionVec * mass1);
}

void Calculate(
	__global int * in_matrix,
	int steps,
	int cores,
	__global float2 * pos,
	__global float2 * dir,
	__global float * mass,
	int planetCount,
	float elapsedTime,
	float simSpeed)
{
	SetRRTMatrix(in_matrix, steps, cores);
	SetPlanetData(pos, dir, mass, planetCount, elapsedTime, simSpeed);
}