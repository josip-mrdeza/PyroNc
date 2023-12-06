struct Vector3
{
    float x;
    float y;
    float z;
};
struct Vector2
{
    float x;
    float y;
};
typedef struct Vector3 vec3;
typedef struct Vector2 vec2;
bool IsInRangeToFix(vec3 tv, vec3 pp, float radius, float verticalMargin)
{
    float distance = sqrt(pow(tv.x + pp.x, 2) + pow(tv.z + pp.z, 2));
    if (distance > radius + 0.3*radius){
        return false;
    }
    if (pp.y > tv.y)
    {
        return false;
    }
    return true;
}
bool IsInRangeToCut(vec3 tv, vec3 pp, float radius, float verticalMargin){
    float distance = sqrt(pow(tv.x + pp.x, 2) + pow(tv.z + pp.z, 2));
    if (distance > radius){
        return false;
    }
    float verticalDistance = fabs((float)(pp.y - tv.y));
    if (verticalDistance > verticalMargin)
    {
        return false;
    }
    return true;
}

void gpu_acc_lh_a(vec3* transformedVertices, vec3* toolPosition, float radius, float verticalMargin)
{
    int i = get_global_id(0);
    vec3 transformedVertex = transformedVertices[i];
    if(!IsInRangeToCut(transformedVertex, toolPosition[0], radius, verticalMargin))
    {
        return;
    }

}