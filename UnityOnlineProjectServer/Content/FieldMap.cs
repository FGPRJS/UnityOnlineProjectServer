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

            var xRegionLength = _x / _xRegionCount;
            var zRegionLength = _z / _zRegionCount;

            nearbyRegions = new Dictionary<int, Region>();

            //Initialize Regions
            regions = new Region[_zRegionCount,_xRegionCount];
            for(int i = 0; i < _zRegionCount; i++)
            {
                for (int j = 0; j < _xRegionCount; j++)
                {
                    var newRegion = new Region(i * j + j, i * zRegionLength, j * zRegionLength, xRegionLength, xRegionLength);

                    regions[i, j] = newRegion;
                }
            }

            //Set NearbyRegions
            for (int i = 0; i < _zRegionCount; i++)
            {
                for (int j = 0; j < _xRegionCount; j++)
                {
                    var targetRegion = regions[i,j];

                    var nearbyLeftRegionIndex = Math.Max(0, i - _nearbyRegionLength);
                    var nearbyRightRegionIndex = Math.Min(_xRegionCount, i + _nearbyRegionLength + 1);
                    var nearbyTopRegionIndex = Math.Max(0, j - _nearbyRegionLength);
                    var nearbyBottomRegionIndex = Math.Min(_zRegionCount, j + _nearbyRegionLength + 1);

                    for (int ri = nearbyLeftRegionIndex; ri < nearbyRightRegionIndex; ri++)
                    {
                        for(int ti = nearbyTopRegionIndex; ti < nearbyBottomRegionIndex; ti++)
                        {
                            var nearbyRegion = regions[ri,ti];
                            //Do not add itself
                            if ((i == ri) && (j == ti)) continue;

                            targetRegion.AddNearbyRegion(nearbyRegion);
                        }
                    }
                }
            }
        }
    }
}
