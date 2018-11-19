using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShip
{
    public class Data
    {
        public int CountPlayer = 0;
        public int CountEnemy = 0;
        public List<Coordenate> FieldForEnemy;
        public bool Touch = false;
        public int TouchCount = 0;
        //public Coordenate OneTouch;
        //public Coordenate EndTouch;
        public List<Coordenate> LikelyCoordinatesForEnemy;
        public Direction Dir;
        public List<Coordenate> DecksShip;
    }
}
