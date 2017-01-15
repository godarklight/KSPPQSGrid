using System;
using System.Collections.Generic;
using UnityEngine;

namespace PQSGrid
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class MainClass : MonoBehaviour
    {
        private bool display = false;
        private List<PQSSquare> squareList = new List<PQSSquare>();
        private const int RADIUS = 1;
        private const double SQUARESIZE = .01d;
        private double lastLatitude = 0d;
        private double lastLongitude = 0d;
        private ScreenMessage posMessage = null;

        public void Update()
        {
            if (FlightGlobals.ready && FlightGlobals.fetch != null && FlightGlobals.fetch.activeVessel != null)
            {
                if (!display)
                {
                    Debug.Log("PQSGRID ENABLED");
                    display = true;
                }
            }
            else if (display)
            {
                Debug.Log("PQSGRID DISABLED");
                display = false;
                DestroySquares();
            }
            if (display)
            {
                Recentre();
                if (posMessage != null)
                {
                    ScreenMessages.RemoveMessage(posMessage);
                    posMessage = null;
                }
                posMessage = ScreenMessages.PostScreenMessage("[ " + FlightGlobals.fetch.activeVessel.latitude + " : " + FlightGlobals.fetch.activeVessel.longitude + " ]", 1f, ScreenMessageStyle.UPPER_CENTER);
                UpdateSquares();
            }
        }

        public void Destory()
        {
            DestroySquares();
        }

        private void Recentre()
        {
            Vessel active = FlightGlobals.fetch.activeVessel;
            double currentLatitude = Math.Round(active.latitude, 1);
            double currentLongitude = Math.Round(active.longitude, 1);
            if (currentLatitude != lastLatitude || currentLongitude != lastLongitude)
            {
                lastLatitude = currentLatitude;
                lastLongitude = currentLongitude;            
                DestroySquares();
                for (int latOffset = -RADIUS; latOffset <= RADIUS; latOffset++)
                {
                    for (int longOffset = -RADIUS; longOffset <= RADIUS; longOffset++)
                    {
                        double createLatitude = currentLatitude + (latOffset * SQUARESIZE);
                        double createLongitude = currentLongitude + (longOffset * SQUARESIZE);
                        PQSSquare square = new PQSSquare(createLatitude, createLongitude, SQUARESIZE);
                        squareList.Add(square);
                    }
                }
            }
        }

        private void UpdateSquares()
        {
            foreach (PQSSquare square in squareList)
            {
                square.Update();
            }
        }

        private void DestroySquares()
        {
            foreach (PQSSquare square in squareList)
            {
                square.Destroy();
            }
            squareList.Clear();
        }
    }
}

