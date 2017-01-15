using System;
using UnityEngine;

namespace PQSGrid
{
    public class PQSSquare
    {
        private GameObject go;
        private LineRenderer lr;
        private double SQUARESIZE;
        //100*100
        private const int RESOLUTION = 100;
        //Offset in metres above the PQS ground
        private const double OFFSET = 0;

        public double latitude
        {
            get;
            private set;
        }

        public double longitude
        {
            get;
            private set;
        }

        public double altitude
        {
            get;
            private set;
        }

        public PQSSquare(double latitude, double longitude, double SQUARESIZE)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            Debug.Log("PQSGRID GENERATING AT [ " + latitude + " : " + longitude + " ]");
            this.SQUARESIZE = SQUARESIZE;
            this.go = new GameObject();
            lr = go.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.SetWidth(.1f, .1f);
            //lr.useWorldSpace = true;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = GenerateMesh();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            Shader testShader = Shader.Find("Transparent/Diffuse"); 
            if (testShader != null)
            {
                Debug.Log("Shader found!");
                mr.material.shader = testShader;
            }
            else
            {
                Debug.LogError("Shader NOT found!");
            }
            Color col = Color.red;
            col.a = 0.2f;
            mr.material.color = col;
            Update();
        }

        public void Update()
        {
            if (go == null)
            {
                //Shouldn't happen, but hey - stranger things have happened with unity I guess?
                return;
            }
            go.transform.position = FlightGlobals.fetch.activeVessel.mainBody.GetWorldSurfacePosition(latitude, longitude, altitude + OFFSET - FlightGlobals.fetch.activeVessel.mainBody.pqsController.radius);
            go.transform.rotation = FlightGlobals.fetch.activeVessel.mainBody.rotation;
            lr.SetPosition(0, FlightGlobals.fetch.activeVessel.GetWorldPos3D());
            double startLat = latitude - (SQUARESIZE / 2d);
            double startLong = longitude - (SQUARESIZE / 2d);
            lr.SetPosition(1, go.transform.position);
        }

        public void Destroy()
        {
            go.DestroyGameObject();
            go = null;
        }

        private Mesh GenerateMesh()
        {
            Mesh newmesh = new Mesh();
            Vector3[] vert = new Vector3[(RESOLUTION + 1) * (RESOLUTION + 1)];
            int[] tri = new int[RESOLUTION * RESOLUTION * 12];
            Vector2[] uv = new Vector2[(RESOLUTION + 1) * (RESOLUTION + 1)];
            //We are using <= because we are also including the vertex edges between squares, otherwise we would miss a row and column of squares
            CelestialBody body = FlightGlobals.fetch.activeVessel.mainBody;
            double startLat = latitude - (SQUARESIZE / 2d);
            double startLong = longitude - (SQUARESIZE / 2d);
            double latitudeRadians = latitude * Mathf.Deg2Rad;
            double longitudeRadians = longitude * Mathf.Deg2Rad;
            Vector3d midRadial = new Vector3d(Math.Cos(latitudeRadians) * Math.Cos(longitudeRadians), Math.Sin(latitudeRadians), Math.Cos(latitudeRadians) * Math.Sin(longitudeRadians));
            //Vector3d midRadial2 = body.GetSurfaceNVector(latitude, longitude);
            altitude = body.pqsController.GetSurfaceHeight(midRadial);
            Vector3d midPos = midRadial * altitude;
            for (int ypos = 0; ypos <= RESOLUTION; ypos++)
            {
                for (int xpos = 0; xpos <= RESOLUTION; xpos++)
                {
                    int vertindex = (ypos * (RESOLUTION + 1)) + xpos;
                    double vertexLong = startLong + (xpos * (SQUARESIZE / (double)RESOLUTION));
                    double vertexLat = startLat + (ypos * (SQUARESIZE / (double)RESOLUTION));
                    double vertexLongRadians = vertexLong * Mathf.Deg2Rad;
                    double vertexLatRadians = vertexLat * Mathf.Deg2Rad;
                    Vector3d vertexRadial = new Vector3d(Math.Cos(vertexLatRadians) * Math.Cos(vertexLongRadians), Math.Sin(vertexLatRadians), Math.Cos(vertexLatRadians) * Math.Sin(vertexLongRadians));
                    //Vector3d vertexRadial = body.GetSurfaceNVector(vertexLat, vertexLong);
                    double vertexAltitude = body.pqsController.GetSurfaceHeight(vertexRadial);
                    Vector3 vertexPosition = (vertexRadial * vertexAltitude) - midPos;
                    vert[vertindex] = vertexPosition;
                    float uvx = xpos / (float)RESOLUTION;
                    float uvy = ypos / (float)RESOLUTION;
                    uv[vertindex] = new Vector2(uvx, uvy);
                    //Drawing past the mesh is no bueno
                    if (xpos != RESOLUTION && ypos != RESOLUTION)
                    {
                        int triindex = ((ypos * RESOLUTION) + xpos) * 12;
                        tri[triindex] = vertindex;
                        tri[triindex + 1] = vertindex + 1;
                        tri[triindex + 2] = vertindex + RESOLUTION + 1;
                        tri[triindex + 3] = vertindex;
                        tri[triindex + 4] = vertindex + RESOLUTION + 1;
                        tri[triindex + 5] = vertindex + 1;
                        tri[triindex + 6] = vertindex + 1;
                        tri[triindex + 7] = vertindex + RESOLUTION + 1;
                        tri[triindex + 8] = vertindex + RESOLUTION + 2;
                        tri[triindex + 9] = vertindex + 1;
                        tri[triindex + 10] = vertindex + RESOLUTION + 2;
                        tri[triindex + 11] = vertindex + RESOLUTION + 1;
                    }
                }
            }
            newmesh.vertices = vert;
            newmesh.triangles = tri;
            newmesh.uv = uv;
            newmesh.Optimize();
            return newmesh;
        }
    }
}

