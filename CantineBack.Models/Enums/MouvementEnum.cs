using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CantineBack.Models.Enums
{
    public enum  MouvementEnum
    {
     
        /// <summary>
        /// Augmenter la quantité de l'article en stock
        /// </summary>
        ENTREE = 0,

        /// <summary>
        /// Diminuer la quantité de l'article en stock
        /// </summary>
        
        SORTIE = 1
    }
}
