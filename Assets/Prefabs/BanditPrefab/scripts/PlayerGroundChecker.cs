using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StealthGame
{
    /// <summary>
    /// Ce composant permet de tester la position du sol sous le personnage.
    /// </summary>
    public class PlayerGroundChecker : MonoBehaviour
    {
        #region Show in inspector

        [Header("Parameters")]

        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _originOffset = 0.5f;
        [SerializeField] private float _originHeight = 0.6f;
        [SerializeField] private float _checkDistance = .8f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private float _groundOffset;

        [Header("Debug")]

        [SerializeField] private bool _drawGizmos;
        [SerializeField] private Color _raycastsColor;
        [SerializeField] private Color _raycastHitsColor;
        [SerializeField] private Color _floorAverageColor;
        [SerializeField] private Color _normalsColor;

        #endregion


        #region Ground check

        /// <summary>
        /// Retourne true ssi le sol est détecté à partir d'une moyenne de multiples tests de raycast.
        /// Le paramètre out averageGroundPosition stocke soit la position moyenne du sol détecté, soit le vecteur 0 s'il n'est pas détecté.
        /// </summary>
        public bool CheckGround(out Vector3 averageGroundPosition)
        {
            // On commence par mettre à jour les points d'origine des raycasts
            UpdateOriginPositions();

#if UNITY_EDITOR
            // (Debug) Tous les points d'impact
            _floorPositions.Clear();
#endif
            // Compteur du nombre de points d'impact
            int hitCount = 0;
            // Initialisation de la position moyenne du sol
            averageGroundPosition = Vector3.zero;

            // On tire un raycast pour chaque point d'origine
            for (int i = 0; i < _originPositions.Length; i++)
            {
                // Si le sol est détecté
                if (CheckGround(_originPositions[i], Vector3.down, out RaycastHit closestHit))
                {
#if UNITY_EDITOR
                    // (Debug) Pour dessiner le point d'impact
                    _floorPositions.Add(closestHit);
#endif
                    // On ajoute le point d'impact et on incrémente le compteur
                    averageGroundPosition += closestHit.point;
                    hitCount++;
                }
            }

            // Si on a eu des impacts avec le sol, on fait la moyenne et on a stocke dans la variable out
            if (hitCount > 0)
            {
                averageGroundPosition /= hitCount;
                averageGroundPosition.y += _groundOffset;
            }

#if UNITY_EDITOR
            // (Debug) Pour dessiner le point moyen
            if (_drawGizmos)
            {
                _groundFound = hitCount > 0;
                _groundPosition = averageGroundPosition;
            }
#endif
            // On retourne enfin si on a eu des points d'impact
            return hitCount > 0;
        }

        /// <summary>
        /// Retourne true ssi le sol est détecté à partir d'un raycast. 
        /// Le paramètre out stocke le RaycastHit s'il y a un point d'impact.
        /// </summary>
        private bool CheckGround(Vector3 origin, Vector3 direction, out RaycastHit closestHit)
        {
            // Version non optimisée
            //return Physics.Raycast(origin, direction, out closestHit, _checkDistance, _layerMask);

            // Version optimisée sans allocation de mémoire
            int hitCount = Physics.RaycastNonAlloc(origin, direction, _hitBuffer, _checkDistance, _layerMask);

            // RaycastNonAlloc ne retourne pas les points d'impact dans l'ordre de distance depuis l'origine.
            // Si on veut le plus proche de l'origine, il faut le calculer avec cette fonction.
            closestHit = GetClosestHit(hitCount, _hitBuffer);

            return hitCount > 0;
        }

        /// <summary>
        /// Retourne le RaycastHit du tableau le plus proche de l'origine du raycast.
        /// </summary>
        private RaycastHit GetClosestHit(int hitCount, RaycastHit[] hits)
        {
            RaycastHit closestHit = new RaycastHit();
            float minDistance = float.PositiveInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].distance < minDistance)
                {
                    closestHit = _hitBuffer[i];
                    minDistance = closestHit.distance;
                }
            }
            return closestHit;
        }

        /// <summary>
        /// Met à jour les positions des origines des raycast en se basant sur le référentiel du joueur.
        /// </summary>
        private void UpdateOriginPositions()
        {
            _originPositions[0] = _playerTransform.TransformPoint(new Vector3(0, _originHeight, 0));
            _originPositions[1] = _playerTransform.TransformPoint(new Vector3(_originOffset, _originHeight, 0));
            _originPositions[2] = _playerTransform.TransformPoint(new Vector3(0, _originHeight, _originOffset));
            _originPositions[3] = _playerTransform.TransformPoint(new Vector3(-_originOffset, _originHeight, 0));
            _originPositions[4] = _playerTransform.TransformPoint(new Vector3(0, _originHeight, -_originOffset));
        }

        #endregion


        #region Debug

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
            {
                return;
            }

            Handles.color = _raycastsColor;

            for (int i = 0; i < _originPositions.Length; i++)
            {
                Vector3 position = _originPositions[i];
                Handles.SphereHandleCap(0, position, Quaternion.identity, .05f, EventType.Repaint);
                Handles.DrawDottedLine(position, position + Vector3.down * _checkDistance, 2f);
            }

            if (Application.isPlaying)
            {
                if (_groundFound)
                {
                    DrawImpactPoints();
                }
            }
            else
            {
                if (CheckGround(out _groundPosition))
                {
                    DrawImpactPoints();
                }
            }

        }

        private void DrawImpactPoints()
        {
            foreach (RaycastHit hit in _floorPositions)
            {
                Handles.color = _raycastHitsColor;
                Handles.SphereHandleCap(0, hit.point, Quaternion.identity, .05f, EventType.Repaint);
                Handles.color = _normalsColor;
                Handles.DrawLine(hit.point, hit.point + hit.normal);
            }
            Handles.color = _floorAverageColor;
            Handles.SphereHandleCap(0, _groundPosition, Quaternion.identity, .1f, EventType.Repaint);
        }

        private bool _groundFound;
        private Vector3 _groundPosition;
        private List<RaycastHit> _floorPositions = new List<RaycastHit>();

#endif
        #endregion


        #region Private

        private Vector3[] _originPositions = new Vector3[5];
        private RaycastHit[] _hitBuffer = new RaycastHit[20];

        #endregion
    }
}