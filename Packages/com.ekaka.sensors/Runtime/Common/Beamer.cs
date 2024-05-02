using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sensors.Main;
using UnityEngine;

namespace Sensors.Common
{
    public class Beamer : Targeter
    {
        private Vector3 Origin => Muzzle.position;
        
        private Vector3 Direction => Muzzle.forward;

        protected override TargetHit[] CastView()
        {
            TargetHit[] hits = new TargetHit[Size];

            Vector3 origin = Origin;
            
            float distance = Range <= 0 ? Mathf.Infinity : Range;
            
            for (int i = 0; i < Size; i++)
            {
                if (Physics.Raycast(origin, Direction, out RaycastHit hitInfo, distance, TargetLayerMask))
                {
                    //skip ignored tag (redo loop)
                    if (IgnoreTagMask.Contains(hitInfo.collider.tag)) i--;
                    
                    else hits[i] = new TargetHit(hitInfo);

                    Vector3 point = hitInfo.point;
                    
                    //remove already cast distance for next cast
                    distance -= Vector3.Distance(point, origin) + Physics.defaultContactOffset;
                    
                    //new origin for next cast!,
                    //add Physics.defaultContactOffset so that cast doesn't hit the same collider again!
                    origin = point + Direction.normalized * Physics.defaultContactOffset;

                    //avoid default value
                    if (distance < 0) distance = 0;
                }

                else break;
            }

            hits = hits.Where(h => h.Collider != null)
                .OrderBy(h => Vector3.Distance(h.Point, Origin)).ToArray();
            
            return hits;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            
            if (Hits != null)
            {
                foreach (TargetHit hit in Hits)
                {
                    Gizmos.DrawLine(Origin, hit.Point);
                    
                    Gizmos.DrawSphere(hit.Point, .125f);
                }
            }
        }
    }
}
