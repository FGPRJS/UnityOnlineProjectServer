using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Content
{
    public class FieldMap
    {
        //Map Size
        //y Axis : UP
        private float _x;
        private float _z;

        //Region Count
        private int _xRegionCount;
        private int _zRegionCount;

        private float _regionWidth;
        private float _regionHeight;

        //Region "Nearby" standard
        private int _nearbyRegionLength;
        //Nearby Regions
        private Dictionary<int, Region> nearbyRegions;

        public Region[,] regions;
        
        public FieldMap(float x, float z, int xRegionCount, int zRegionCount, int nearbyRegionLength)
        {
            _x = x;
            _z = z;
            _xRegionCount = xRegionCount;
            _zRegionCount = zRegionCount;
            _nearbyRegionLength = nearbyRegionLength;

            _regionWidth = _x / _xRegionCount;
            _regionHeight = _z / _zRegionCount;

            nearbyRegions = new Dictionary<int, Region>();

            //Initialize Regions
            regions = new Region[_zRegionCount,_xRegionCount];
            for(int i = 0; i < _zRegionCount; i++)
            {
                for (int j = 0; j < _xRegionCount; j++)
                {
                    var newRegion = new Region(i * j + j, j * _regionWidth, i * _regionHeight, _regionWidth, _regionHeight);

                    newRegion.GameObjectLostEvent += MoveGameObjectToAppropriateRegion;

                    regions[i, j] = newRegion;
                }
            }

            //Set NearbyRegions
            for (int i = 0; i < _zRegionCount; i++)
            {
                for (int j = 0; j < _xRegionCount; j++)
                {
                    var targetRegion = regions[i,j];

                    //x
                    var xMin = Math.Max(0, i - _nearbyRegionLength);
                    var xMax = Math.Min(_xRegionCount, i + _nearbyRegionLength + 1);
                    //z
                    var zMin = Math.Max(0, j - _nearbyRegionLength);
                    var zMax = Math.Min(_zRegionCount, j + _nearbyRegionLength + 1);

                    for (var zIndex = zMin; zIndex < zMax; zIndex++)
                    {
                        for (var xIndex = xMin; xIndex < xMax; xIndex++)
                        {
                            var nearbyRegion = regions[zIndex,xIndex];
                            //Do not add itself
                            if ((i == zIndex) && (j == xIndex)) continue;

                            targetRegion.AddNearbyRegion(nearbyRegion);
                        }
                    }
                }
            }
        }
        
        public void AddObject(GameObject obj)
        {
            var region = GetAppropriateRegion(obj);

            region?.AddGameObject(obj);
        }

        private void MoveGameObjectToAppropriateRegion(object sender, GameObject obj)
        {
            GetAppropriateRegion(obj).AddGameObject(obj);
        }

        public Region GetAppropriateRegion(GameObject obj)
        {
            int objXIndex = (int)(Math.Ceiling(obj.Position.X / _regionWidth));
            int objZIndex = (int)(Math.Ceiling(obj.Position.Z / _regionHeight));

            try
            {
                var result = regions[objZIndex, objXIndex];
                return result;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }
    }
}
