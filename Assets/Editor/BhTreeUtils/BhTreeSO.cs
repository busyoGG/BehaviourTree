using System.Collections.Generic;
using UnityEngine;

namespace BhTreeUtils
{
   [CreateAssetMenu(menuName = "BhTree/BhTreeSO")]
   public class BhTreeSO : ScriptableObject
   {
      private SList<object> bhTree = new SList<object>();
   }

}