using System;
using System.Linq;
using Core.Utils;
using Sensors.Main;
using UnityEngine;

namespace Sensors.Utils
{
    public class Vision : Targeter
    {
        [field: Space]
        
        [field: Range(0f, 180f)]
        [field: SerializeField] public float VerticalFov { get; private set; } = 90f;

        [field: Range(0f, 180f)]
        [field: SerializeField] public float HorizontalFov { get; private set; } = 90f;

        [field: Space] 
        
        //fields used for calculating frustum
        #region Geometry Fields

        private Vector3 _origin;
        
        private Vector3 _forward;
        
        private float _halfWidth;
        
        private float _halfHeight;
        
        //calculated frustum planes cached for calculation
        private Plane[] _calculatedPlanes;
        //far plane (front most plane of the frustum) cached for checking if collider is beyond frustum view
        private Plane _farPlane;

        #endregion
        
        /// <summary>
        /// cast a frustum view based on given fields
        /// </summary>
        /// <returns>objects in frustum and line of sight</returns>
        protected override TargetHit[] CastView()
        {
            Collider[] results = new Collider[Size];

            //get half fov in rad to get width and height
            float fovX = (HorizontalFov / 2f) * Mathf.Deg2Rad;

            float fovY = (VerticalFov / 2f) * Mathf.Deg2Rad;

            _halfWidth = (Mathf.Tan(fovX) * Range) / 2f;

            _halfHeight = (Mathf.Tan(fovY) * Range) / 2f;

            _origin = Muzzle.position;

            _forward = Muzzle.forward;
            
            Vector3 center = _origin + (_forward * (Range / 2f));

            Vector3 halfExtents = new Vector3(_halfWidth, _halfHeight, Range / 2f);
            
            int hits = Physics.OverlapBoxNonAlloc(center, halfExtents, results, Muzzle.rotation, TargetLayerMask);
            
            if (hits <= 0)
            {
                results = new Collider[0];

                return Array.ConvertAll(results, c => new TargetHit(c));
            }

            _calculatedPlanes = CalculatePlanes();
            //filter and assign results
            results = results.Where(c => c != null && !IgnoreTagMask.Contains(c.tag) && IsInFrustum(c) && IsInLineOfSight(c)).ToArray();
            //arrange/order by distance from viewer/eye
            results = results.OrderBy(c => Vector3.Distance(c.bounds.center, Muzzle.position)).ToArray();

            return Array.ConvertAll(results, c => new TargetHit(c));
        }

        private bool IsInLineOfSight(Collider result)
        {
            Bounds bounds = result.bounds;
            
            Vector3[] vertices = bounds.GetBoundVertices();
            //append center
            vertices = vertices.Append(bounds.center).ToArray();

            //get vertices only inside frustum
            vertices = vertices.Where(v => _calculatedPlanes.All(p => p.GetSide(v))).ToArray();
            
            foreach (var vertex in vertices)
            {
                //max cast distance
                float maxDistance = Vector3.Distance(_origin, vertex);

                bool hit = Physics.Raycast(_origin, vertex - _origin, out RaycastHit hitInfo, maxDistance);

                if (hit)
                {
                    //if hit was result it means it's in line of sight
                    if (hitInfo.collider == result)
                    {
                        return true;
                    }
                    
                    //else check next vertex
                    continue;
                }

                //if there's no hit it means line of sight is clear,
                //this will happen since bounding box vertices is on the edge so rayCasts could miss
                return true;
            }
            //if nothing hit return false
            return false;
        }
        
        private bool IsInFrustum(Collider result)
        {
            //is collider beyond frustum view
            //important to check because GeometryUtility.TestPlanesAABB considers everything in frustum view without range
            if (!_farPlane.GetSide(result.bounds.center))
            {
                return false;
            }
            
            return GeometryUtility.TestPlanesAABB(_calculatedPlanes, result.bounds);
        }
        
        //calculate frustum planes/sides
        private Plane[] CalculatePlanes()
        {
            Vector3 forward = _forward * Range;
            
            Vector3 right = Muzzle.right;
            
            Vector3 up = Muzzle.up;
            
            Vector3 topRight = _origin + forward + right * _halfWidth + up * _halfHeight;
            
            Vector3 topLeft = _origin + forward - right * _halfWidth + up * _halfHeight;
            
            Vector3 bottomRight = _origin + forward + right * _halfWidth - up * _halfHeight;
            
            Vector3 bottomLeft = _origin + forward - right * _halfWidth - up * _halfHeight;
            
            //cache farPlane
            _farPlane = new Plane(topLeft, topRight, bottomLeft);

            return new []
            {
                //left plane
                new Plane(_origin, topLeft, bottomLeft), 
                //right plane
                new Plane(_origin, bottomRight, topRight), 
                
                //top plane
                new Plane(_origin, topRight, topLeft), 
                //bottom plane
                new Plane(_origin, bottomLeft, bottomRight),
                
                _farPlane,
                //near plane
                new Plane(_origin, _origin + right, _origin + up), 
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (Muzzle != null)
            {
                Gizmos.color = Color.red;

                DrawViewFrustum();
            }
        }
        
        public void DrawViewFrustum()
        {
            //this is calculated in edit mode and every frame in play mode
            //fields are cached when frustum is cast so can't use every frame
            Vector3 origin = Muzzle.position;
            
            Vector3 forward = Muzzle.forward * Range;
            
            Vector3 right = Muzzle.right;
            
            Vector3 up = Muzzle.up;
            
            float fovX = (HorizontalFov / 2f) * Mathf.Deg2Rad;

            float fovY = (VerticalFov / 2f) * Mathf.Deg2Rad;

            float halfWidth = (Mathf.Tan(fovX) * Range) / 2f;

            float halfHeight = (Mathf.Tan(fovY) * Range) / 2f;

            Vector3 topRight = origin + forward + (right * halfWidth) + (up * halfHeight);
            
            Vector3 topLeft = origin + forward - (right * halfWidth) + (up * halfHeight);
            
            Vector3 bottomRight = origin + forward + (right * halfWidth) - (up * halfHeight);
            
            Vector3 bottomLeft = origin + forward - (right * halfWidth) - (up * halfHeight);
            
            //drawLines to frustum
            Gizmos.DrawLine(origin, topRight);
            Gizmos.DrawLine(origin, topLeft);
            
            Gizmos.DrawLine(origin, bottomRight);
            Gizmos.DrawLine(origin, bottomLeft);
            
            //draw front box
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.DrawLine(topRight, bottomRight);
        }
    }
}
