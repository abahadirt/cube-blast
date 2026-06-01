namespace Blast.Core.Data
{
    public class CubeData
    {
        public int Health;
        public CubeColor Color;
        public int Column;

        public CubeData(int column, CubeColor color, int health = 1)
        {
            Column = column;
            Color = color;
            Health = health;
        }

        public bool TakeDamage(int damage)
        {
            Health -= damage;
            return Health <= 0;
        }

        public void SetColor(CubeColor newColor) => Color = newColor;
    }
}