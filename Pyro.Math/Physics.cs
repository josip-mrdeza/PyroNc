namespace Pyro.Math
{
    public static partial class Physics
    {
        /// <summary>
        /// Calculates the force required to push an object from it's starting position to the end position if it's mass and velocity are constant.
        /// </summary>
        /// <returns>The force required to push an object from position '<see cref="p1"/>' to position '<see cref="p2"/>' in 2D space.</returns>
        public static float ForceRequiredToPush(Vector2D p1, Vector2D p2, float mass = 1f, float velocity = 1f)
        {
            return (0.5f * mass * velocity.Squared()) / Space2D.Distance(p1, p2);
        }
        
        /// <summary>
        /// Calculates the force required to push an object from it's starting position to the end position if it's mass and velocity are constant.
        /// </summary>
        /// <returns>The force required to push an object from position '<see cref="p1"/>' to position '<see cref="p2"/>' in 3D space.</returns>
        public static float ForceRequiredToPush(Vector3D p1, Vector3D p2, float mass = 1f, float velocity = 1f)
        {
            return (0.5f * mass * velocity.Squared()) / Space3D.Distance(p1, p2);
        }

        public static float VelocityWhilePushing(float force, float distance, float mass = 1f)
        {
            return ((force * distance * 2) / mass).SquareRoot();
        }
    }
}