using System.Collections.Generic;


namespace AGTCore
{
    public static class EntityManager
    {
        public static List<Entity> entities = new List<Entity>();
        public static Queue<Entity> entitiesToInit = new Queue<Entity>();

        public static void RenderEntities()
        {
            while (entitiesToInit.TryDequeue(out Entity entity))
            {
                entity.InitMesh();
                entities.Add(entity);
            }
            for (int x = 0; x < entities.Count; x++)
            {
                entities[x].RenderMesh();
            }
        }
    }
}