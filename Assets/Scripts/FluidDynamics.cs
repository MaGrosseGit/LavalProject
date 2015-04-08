using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FluidDynamics : MonoBehaviour {

    /**********************************************************************/
    public float windMultiplier;
    public int numParticles = 256; //static
    public ParticleTypes[] particleTypes;
    public Particle[] particles;
    public static float v = 0.033f; //1 / 30;

    public static float GRAVITY = -1f;
    public static float BOUNCE_DAMPENING = 3f; //1 / 64;
    public float radius = 5;

    //public Color decay = new Color(0, 0, 0, 16); // black with 16/256 % opacity

    public NavierStokesSolver fluidSolver;
    public float visc, diff, vScale, velocityScale;
    public float limitVelocity;
    private float oldMouseX = 1, oldMouseY = 1;

    public bool vectors = true;

    //public Color c = new Color(64, 128, 256); // particle pixel color

    public int width = 0;
    public int height = 0;

    public Material trailMat;
    Material quadMat;

    private float dt = 0f;

    public bool grav = false;

    float mouseX = 0;
    float mouseY = 0;

    GameObject[] triangles;
    public int numOfRows = 16;
    public float scale = 100;

    List<int> cellsX = new List<int>();
    List<int> cellsY = new List<int>();
    Vector3[] vectorCellsDir;
    Vector3[] vectorCellsOrigin;

    //List<int> listId = new List<int>();


    int densityBrushSize = 0;   // Size of the density area applied with the mouse
    int velocityBrushSize = 0;  // Ditto velocity
    int lineSpacing = 0;        // Spacing between velocity and normal lines

    Vector3 onePixel;

    List<GameObject> quads = new List<GameObject>();

    int addedX = 0;
    int addedY = 0;
    int addedZ = 0;

    GameObject particleOffset;
    Transform minTrans;
    Transform maxTrans;
    Vector3 minOffset;
    Vector3 maxOffset;

    public Rotation target;
    //public GameObject firstTarget;
    //public Canvas secondTarget;

    public bool useParticleTypes = false;

    void Start ()
    {
        if (transform.FindChild("_ParticlesOffset"))
        {
            particleOffset = GameObject.Find("_ParticlesOffset");
            minTrans = particleOffset.transform.FindChild("min");
            maxTrans = particleOffset.transform.FindChild("max");
            minOffset = minTrans.position;
            maxOffset = maxTrans.position;
        }
        else
        {
            minOffset = Vector3.zero;
            maxOffset = Vector3.zero;
        }

        //width = Screen.width;
        //height = Screen.height;

        Vector3 distToCamV = transform.position - Camera.main.transform.position;
        float distToCamF = Vector3.Dot(distToCamV, Camera.main.transform.forward);

        downLeftPos = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, distToCamF));
        downRightPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, distToCamF));
        upLeftPos = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, distToCamF));
        upRightPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, distToCamF));

        //transform.parent.position = downLeftPos;

        //Vector3 distWidth = downLeftPos - downRightPos;
        //float newWidth = Vector3.Dot(distWidth, Camera.main.transform.forward);
        //Vector3 distHeight = upLeftPos - downLeftPos;
        //float newHeight = Vector3.Dot(distHeight, Camera.main.transform.forward);

        width = (int)Mathf.Ceil(Vector3.Distance(downLeftPos, downRightPos));
        height = (int)Mathf.Ceil(Vector3.Distance(upLeftPos, downLeftPos));

        //Vector3 dist = transform.parent.position - Camera.main.transform.position;
        //float distT = Vector3.Dot(dist, Camera.main.transform.forward);

        //Vector3 parentPos = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, distT));
        //transform.parent.position = parentPos;

        //Vector3 pixelOne = Camera.main.ScreenToWorldPoint(new Vector3(1, 1, distT));
        //float pixelSize = Vector3.Distance(parentPos, pixelOne);
        //onePixel = new Vector3(pixelSize, pixelSize, 0);

        int oldNumParticles = numParticles;
        if(particleTypes.Length > 0){
            numParticles = 0;
            for (int i = 0; i < particleTypes.Length; i++)
            {
                numParticles += particleTypes[i].numOfParticles;
            }
        }

        if (numParticles == 0)
            numParticles = oldNumParticles;
        else
            useParticleTypes = true;
        particles = new Particle[numParticles];

        densityBrushSize = numOfRows / 10; 
        velocityBrushSize = numOfRows / 20; 
        lineSpacing = numOfRows / 20;
        vectorCellsDir = new Vector3[numOfRows * numOfRows];
        vectorCellsOrigin = new Vector3[numOfRows * numOfRows];

        fluidSolver = new NavierStokesSolver(numOfRows);
        visc = 0.0001f;
        diff = 0.03f;
        vScale = 0;
        //velocityScale = 50;

        initParticles();

        //triangles = new GameObject[NavierStokesSolver.SIZE];
        //drawMotionVectorsImmediate(vScale * 1.4f, true);*/

        //visc = 0.01f;
        //diff = 0.02f;

        //limitVelocity = 200;
    }

    Vector3 downLeftPos;
    Vector3 downRightPos;
    Vector3 upLeftPos;
    Vector3 upRightPos;

    //public ShroudInstance cloth;

    void Update ()
    {
        if (GameObject.Find("_ParticlesOffset"))
        {
            minOffset = minTrans.position;
            maxOffset = maxTrans.position;
        }
        else
        {
            minOffset = Vector3.zero;
            maxOffset = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //Screen.showCursor = false;
        //secondTarget.transform.position = firstTarget.transform.position;
        //Vector2 pos;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(secondTarget.transform as RectTransform, Input.mousePosition, secondTarget.worldCamera, out pos);
        //secondTarget.transform.position = secondTarget.transform.TransformPoint(pos);
        //Debug.Log(Input.mousePosition.x + "," + Input.mousePosition.y);

        //Debug.Log("MOUSE INPUT = " + new Vector2(mouseX, mouseY));


        //if (width != Screen.width || height != Screen.height)
        //{
        //    fluidSolver = new NavierStokesSolver(numOfRows);
        //    width = Screen.width;
        //    height = Screen.height;

        //    Vector3 dist = transform.parent.position - Camera.main.transform.position;
        //    float distT = Vector3.Dot(dist, Camera.main.transform.forward);

        //    Vector3 parentPos = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, distT));
        //    transform.parent.position = parentPos;
        //}


        Vector3 distToCamV = transform.position - Camera.main.transform.position;
        float distToCamF = Vector3.Dot(distToCamV, Camera.main.transform.forward);

        //mouseX = Input.mousePosition.x;
        //mouseY = Input.mousePosition.y;
        mouseX = EyePosScript.realEyesPos.x;
        mouseX = EyePosScript.realEyesPos.y;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mouseX, mouseY, distToCamF));
        mousePos = EyePosScript.eyesPos;
        mousePos = mousePos - new Vector3(addedX, addedY, addedZ);
        mouseX = mousePos.x;
        mouseY = mousePos.y;

        handleMouseMotion(windMultiplier);

        downLeftPos = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, distToCamF));
        downRightPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, distToCamF));
        upLeftPos = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, distToCamF));
        upRightPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, distToCamF));

        //Vector3 distWidth = downLeftPos - downRightPos;
        //float newWidth = Vector3.Dot(distWidth, Camera.main.transform.forward);
        //Vector3 distHeight = upLeftPos - downLeftPos;
        //float newHeight = Vector3.Dot(distHeight, Camera.main.transform.forward);

        //float newWidth = (int)Mathf.Ceil(Vector3.Distance(downLeftPos, downRightPos));
        //float newHeight = (int)Mathf.Ceil(Vector3.Distance(upLeftPos, downLeftPos));

        int oldWidth = width;
        int oldHeight = height;

        width = (int)Mathf.Ceil(Vector3.Distance(downLeftPos, downRightPos));
        height = (int)Mathf.Ceil(Vector3.Distance(upLeftPos, downLeftPos));

        if (Input.GetKeyDown(KeyCode.UpArrow))
            transform.position = upLeftPos;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            transform.position = downLeftPos;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            transform.position = upRightPos;
        if (Input.GetKeyDown(KeyCode.RightArrow))
            transform.position = downRightPos;

        //Debug.Log("W,H = " + new Vector2(newWidth, newHeight));
        //Debug.Log("W,H = " + new Vector2(width, height));
        //Debug.DrawRay(transform.position, new Vector3(width, 0, 0), Color.green);
        //Debug.DrawRay(transform.position, new Vector3(0, height, 0), Color.green);
        
        addedX = (int)Mathf.Ceil(downLeftPos.x);
        addedY = (int)Mathf.Ceil(downLeftPos.y);
        addedZ = (int)Mathf.Ceil(downLeftPos.z);

        if (oldWidth != width || oldHeight != height)
        {
            //Debug.Log("NEW RESOLUTION");
            fluidSolver = new NavierStokesSolver(numOfRows);
        }


	    dt=Time.smoothDeltaTime;
        fluidSolver.Tick(dt, visc, diff);
		
        //drawMotionVectorsImmediate( vScale*1.4f);

        //vScale = velocityScale * (60f * (1.0f / Time.smoothDeltaTime));
        vScale = velocityScale * dt;

        drawParticles();
        //fluidSolver.Tick(dt, visc, diff);

        if (vectors)
        {
            paintGrid();
        }
        paintMotionVector(scale);

        cellsX.Clear();
        cellsY.Clear();
        //listId.Clear();

        //setForce(fluidSolver);
        //drawField(fluidSolver);
    }


    private void paintMotionVector(float scale)
    {
        int n = NavierStokesSolver.N;
        float cellHeight = height / n;
        float cellWidth = width / n;

        int arrayCount = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                float dx = (float)fluidSolver.getDx(i, j);
                float dy = (float)fluidSolver.getDy(i, j);

                float x = cellWidth / 2 + cellWidth * i;
                float y = cellHeight / 2 + cellHeight * j;

                int cellX = (int)Mathf.Floor(x / cellWidth);
                int cellY = (int)Mathf.Floor(y / cellHeight);

                dx *= scale;
                dy *= scale;

                //(listId.Contains(cellX + cellY)) 
                Vector3 cellVectorStart = new Vector3(x + addedX, y + addedY, addedZ);
                Vector3 cellVectorDir = new Vector3(x + dx + addedX, y + dy + addedY, addedZ);

                vectorCellsDir[arrayCount] = cellVectorDir;
                vectorCellsOrigin[arrayCount] = cellVectorStart;

                if (vectors)
                {
                    if (cellsX.Contains(cellX) && cellY == cellsY[cellsX.IndexOf(cellX)])
                    {
                        Debug.DrawLine(cellVectorStart, cellVectorDir, Color.cyan);
                        //Debug.DrawLine(new Vector3(x, y, 0), new Vector3(x + Mathf.Clamp(dx, 0, 20), y + Mathf.Clamp(dy, 0, 20), 0), Color.cyan);
                    }
                    else
                    {
                        Debug.DrawLine(cellVectorStart, cellVectorDir, Color.red);
                        //Debug.DrawLine(new Vector3(x, y, 0), new Vector3(x + Mathf.Clamp(dx, 0, 20), y + Mathf.Clamp(dy, 0, 20), 0), Color.red);
                    }
                }
                arrayCount++;
            }
        }
    }

    private void paintGrid()
    {
        int n = NavierStokesSolver.N;
        float cellHeight = height / n;
        float cellWidth = width / n;
        for (int i = 0; i <= n; i++)
        {
            //line(0, cellHeight * i, width, cellHeight * i);
            //Debug.DrawLine(new Vector3(0, cellHeight * i, 0), new Vector3(width, cellHeight * i, 0), Color.gray);
            Debug.DrawLine(new Vector3(addedX, (cellHeight * i) + addedY, addedZ), new Vector3(width + addedX, (cellHeight * i) + addedY, addedZ), Color.gray);
            //line(cellWidth * i, 0, cellWidth * i, height);
            //Debug.DrawLine(new Vector3(cellWidth * i, 0, 0), new Vector3(cellWidth * i, height, 0), Color.blue);
            Debug.DrawLine(new Vector3((cellWidth * i) + addedX, addedY, addedZ), new Vector3((cellWidth * i) + addedX, height + addedY, addedZ), Color.blue);

        }
    }

    private void drawMotionVectorsImmediate(float l, bool init = false)
    {
        int n = NavierStokesSolver.N;
        float cellHeight = height / n;
        float cellWidth = width / n;
        float dx, dy, x, y, x1, y1, x2, y2, x3, y3;
        int i, j;

        float thick = 0.1f;

        for (i = 0; i < n; i++)
        {
            for (j = 0; j < n; j++)
            {

                int triangleId = i + (n + 2) * j;

                dx = fluidSolver.getDx(i, j);
                dy = fluidSolver.getDy(i, j);

                x = cellWidth / 2 + cellWidth * i;
                y = cellHeight / 2 + cellHeight * j;

                x1 = x + dx * l;
                y1 = y + dy * l;

                x2 = x + dy * l * thick;
                y2 = y - dx * l * thick;

                x3 = x - dy * l * thick;
                y3 = y + dx * l * thick;

                if (init)
                {
                    // normal(0, 0, 1f);
                    /*vertex(x1, y1);
                    vertex(x2, y2);
                    vertex(x3, y3); */
                 
                    GameObject triangle = new GameObject();
                    triangle.AddComponent<MeshFilter>();
                    triangle.AddComponent<MeshRenderer>();
                    Mesh mesh = triangle.GetComponent<MeshFilter>().mesh;
                    mesh.Clear();
                    mesh.vertices = new Vector3[] { new Vector3(x1, y1, 0), new Vector3(x2, y2, 0), new Vector3(x3, y3, 0) };
                    mesh.uv = new Vector2[] { new Vector2(x1, y1), new Vector2(x2, y2), new Vector2(x3, y3) };
                    mesh.triangles = new int[] { 0, 1, 2 };
                    triangle.GetComponent<Renderer>().material = trailMat;

                    GameObject parent = GameObject.Find("TriParent");
                    triangle.transform.parent = parent.transform;
                    triangles[triangleId] = triangle;
                }
                else
                {
                    GameObject triangle = triangles[triangleId];
                    Mesh mesh = triangle.GetComponent<MeshFilter>().mesh;
                    mesh.vertices = new Vector3[] { new Vector3(x1, y1, 0), new Vector3(x2, y2, 0), new Vector3(x3, y3, 0) };
                    //mesh.uv = new Vector2[] { new Vector2(x1, y1), new Vector2(x2, y2), new Vector2(x3, y3) };
                    //mesh.triangles = new int[] { 0, 1, 2 };
                    mesh.RecalculateBounds();
                }
            }
        }
    }

    private void handleMouseMotion(float multiplier)
    {
        mouseX = Mathf.Max(1, mouseX);
        mouseY = Mathf.Max(1, mouseY);

        int n = NavierStokesSolver.N;
        float cellHeight = height / n;
        float cellWidth = width / n;

        float mouseDx = (mouseX - oldMouseX) * multiplier;
        float mouseDy = (mouseY - oldMouseY) * multiplier;
        int cellX = (int) Mathf.Floor(mouseX / cellWidth);
        int cellY = (int)Mathf.Floor(mouseY / cellHeight);


        mouseDx = (Mathf.Abs(mouseDx) > limitVelocity) ? Mathf.Sign(mouseDx) * limitVelocity : mouseDx;
        mouseDy = (Mathf.Abs(mouseDy) > limitVelocity) ? Mathf.Sign(mouseDy) * limitVelocity : mouseDy;

        if (target != null)
        {
            float newSpeed = 0;
            if(mouseDy != 0 && mouseDx != 0)
                newSpeed = (mouseDx * mouseDy);
            else if (mouseDx == 0 && mouseDy != 0)
                newSpeed = (mouseDy * 2);
            else if (mouseDx != 0 && mouseDy == 0)
                newSpeed = (mouseDx * 2);
            newSpeed *= 1000;
            newSpeed = Mathf.Clamp(newSpeed, 0, 1500);

            float lerpSpeed = 0;

            if (newSpeed == 0)
                lerpSpeed = .5f;
            else
                lerpSpeed = 20f;

            target.speed = Mathf.Lerp(target.speed, newSpeed, lerpSpeed * Time.deltaTime);
            //Debug.Log("TS = "+target.speed+", NS = "+newSpeed);
        }

        //Debug.Log(new Vector2(mouseX, mouseY));

        if (Input.GetKey(KeyCode.LeftControl))
            NavierStokesSolver.bounds[fluidSolver.INDEX(cellX, cellY)] = true;

        //Debug.Log("X = " + cellX + ", Y = " + cellY+", B = "+NavierStokesSolver.bounds[fluidSolver.INDEX(cellX, cellY)]);

        if (Input.mousePosition.x <= Screen.width && Input.mousePosition.y <= Screen.height-20)
        {
            fluidSolver.applyForce(cellX, cellY, mouseDx, mouseDy);
            //Debug.Log("Mouse Pos = " + new Vector2(Input.mousePosition.x, Input.mousePosition.y) + "Screen = " + new Vector2(Screen.width, Screen.height));
        }

        oldMouseX = mouseX;
        oldMouseY = mouseY;
    }

    private void initParticles()
    {
        GameObject parent = new GameObject();
        parent.name = "ParticlesParent" + ((int)UnityEngine.Random.Range(0, 1000));
        if (!useParticleTypes)
        {
            for (int i = 0; i < numParticles; i++)
            {
                float x = UnityEngine.Random.Range(0, width);
                float y = UnityEngine.Random.Range(0, height);
                particles[i] = new Particle(x, y, radius);
                particles[i].charge = (float)0.5;

                float ladung = particles[i].charge;
                Color32 particleColor = new Color32((byte)(128 + (ladung - 0.5) * 255), 128,
                        (byte)(128 - (ladung - 0.5) * 255), 255);
                int d = (int)(particles[i].radius * 2);
                GameObject ellipse = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ellipse.transform.position = new Vector3((float)particles[i].posX, (float)particles[i].posY, 0);
                ellipse.transform.localScale = new Vector3(radius, radius, radius);
                //ellipse.GetComponent<BoxCollider>().enabled = false;
                ellipse.GetComponent<Renderer>().material.color = particleColor;
                ellipse.GetComponent<MeshRenderer>().enabled = false;
                ellipse.AddComponent<TrailRenderer>();
                ellipse.GetComponent<TrailRenderer>().material = trailMat;
                ellipse.GetComponent<TrailRenderer>().startWidth = 1.5f;
                ellipse.GetComponent<TrailRenderer>().endWidth = 0.5f;
                ellipse.GetComponent<TrailRenderer>().time = 1.5f;
                ellipse.name = "p" + i;

                /*ellipse.AddComponent<Rigidbody>();
                ellipse.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;*/

                ellipse.transform.parent = parent.transform;

                particles[i].particleGO = ellipse;
            }
        }
        else
        {
            int particleCount = 0;
            for (int i = 0; i < particleTypes.Length; i++)
            {
                ParticleTypes curParticleType = particleTypes[i];
                GameObject curParent = new GameObject();
                curParent.name = "ParentN" + i;
                curParent.transform.parent = parent.transform;
                for (int k = 0; k < curParticleType.numOfParticles; k++)
                {
                    //Debug.Log(particleCount);
                    float x = UnityEngine.Random.Range(0, width);
                    float y = UnityEngine.Random.Range(0, height);
                    particles[particleCount] = new Particle(x, y, radius);
                    Particle curParticle = particles[particleCount];

                    if(!curParticleType.isWind)
                        curParticle.particleGO = Instantiate(curParticleType.prefabGo, Vector3.zero, Quaternion.identity) as GameObject;
                    else
                        curParticle.particleGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    GameObject curParticleGO = curParticle.particleGO;

                    curParticle.charge = (float)0.5;
                    curParticleGO.transform.position = new Vector3((float)curParticle.posX, (float)curParticle.posY, 0);
                    if(curParticleType.multiplyScale)
                        curParticleGO.transform.localScale = new Vector3(curParticleGO.transform.localScale.x * curParticleType.radius, 
                            curParticleGO.transform.localScale.y * curParticleType.radius,
                            curParticleGO.transform.localScale.z * curParticleType.radius);
                    else
                        curParticleGO.transform.localScale = new Vector3(curParticleType.radius, curParticleType.radius, curParticleType.radius);

                    if(curParticleGO.GetComponent<Collider>())
                        curParticleGO.GetComponent<Collider>().enabled = false;
                    if(curParticleType.particleMat != null)
                        curParticleGO.GetComponent<Renderer>().material = curParticleType.particleMat;
                    if(curParticleType.isWind)
                        curParticleGO.GetComponent<MeshRenderer>().enabled = false;
                    if (curParticleType.rotate)
                    {
                        if (!curParticleGO.GetComponent<Rotation>())
                            curParticleGO.AddComponent<Rotation>();
                        curParticleGO.GetComponent<Rotation>().speed = curParticleType.rotationSpeed;
                        if (curParticleType.randomRot)
                        {
                            curParticleGO.GetComponent<Rotation>().axis = new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value); 
                        }
                        else
                            curParticleGO.GetComponent<Rotation>().axis = curParticleType.rotDirection;
                    }
                    if(curParticleType.randomColor)
                        curParticleGO.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    if (curParticleType.hasTrail)
                    {
                        if (!curParticleGO.GetComponent<TrailRenderer>())
                            curParticleGO.AddComponent<TrailRenderer>();
                        curParticleGO.GetComponent<TrailRenderer>().material = curParticleType.trailType.trailMat;
                        curParticleGO.GetComponent<TrailRenderer>().startWidth = curParticleType.trailType.startWidth;
                        curParticleGO.GetComponent<TrailRenderer>().endWidth = curParticleType.trailType.endWidth;
                        curParticleGO.GetComponent<TrailRenderer>().time = curParticleType.trailType.time;
                        curParticleGO.GetComponent<TrailRenderer>().receiveShadows = false;
                        curParticleGO.GetComponent<TrailRenderer>().castShadows = false;
                    }
                    curParticleGO.name = "p" + k;
                    curParticleGO.transform.parent = curParent.transform;

                    curParticle.gravityForce = curParticleType.gravityForce;
                    curParticle.gravitySpeed = curParticleType.gravitySpeed;
                    curParticle.minSpeedToGravity = curParticleType.minSpeedToGravity;
                    curParticle.maxVelocity = curParticleType.maxVelocity;
                    curParticle.particleMat = curParticleType.particleMat;

                    particleCount++;
                }
            }

        }
    }

    private void drawParticles()
    {
        particles[0].charge = 0;
        particles[1].charge = 1;
        updateParticles();

        /*for (int i = 0; i < numParticles; i++)
        {
            //particles[i].posY = Mathf.Clamp(particles[i].posY, 0, height);
            particles[i].posY = Mathf.Clamp(particles[i].posY, 0, height);
            //particles[i].posX = Mathf.Clamp(particles[i].posX, 0, width);
            particles[i].posX = Mathf.Clamp(particles[i].posX, 0, width);
            int d = (int)(particles[i].radius * 2);
            GameObject ellipse = particles[i].particleGO;
            //Debug.Log("W,H = " + new Vector2(particles[i].posX, particles[i].posY));
            //ellipse.transform.position = new Vector3((float)particles[i].posX , (float)particles[i].posY, 0);
            Vector3 ellipsePos = new Vector3((float)particles[i].posX + addedX, (float)particles[i].posY + addedY, addedZ);
            ellipsePos = new Vector3(Mathf.Clamp(ellipsePos.x, minOffset.x, maxOffset.x), Mathf.Clamp(ellipsePos.y ,minOffset.y, maxOffset.y),ellipsePos.z);
            ellipse.transform.position = ellipsePos;

            //ellipse.transform.localScale = new Vector3(d, d, 0);
            //ellipse((float)particles[i].posX, (float)particles[i].posY, d, d);
        }*/
        int n = NavierStokesSolver.N;
        float cellHeight = height / n;
        float cellWidth = width / n;


        for (int i = 0; i < numParticles; i++)
        {
            particles[i].posY = Mathf.Clamp(particles[i].posY, 0, height);
            particles[i].posX = Mathf.Clamp(particles[i].posX, 0, width);
            int d = (int)(particles[i].radius);
            GameObject curParticle = particles[i].particleGO;
            //Debug.Log("W,H = " + new Vector2(particles[i].posX, particles[i].posY));
            //ellipse.transform.position = new Vector3((float)particles[i].posX , (float)particles[i].posY, 0);

            /*if (!particles[i].particleGO.GetComponent<Rigidbody>().isKinematic)
                particles[i].particleGO.GetComponent<Rigidbody>().isKinematic = true;*/
            if (float.IsNaN(particles[i].posX))
                particles[i].posX = particles[i].prevPosX;
            if (float.IsNaN(particles[i].posY))
                particles[i].posY = particles[i].prevPosY;
            //Debug.Log(new Vector2(particles[i].posX, particles[i].posY));
            Vector3 particlePos = new Vector3((float)particles[i].posX + addedX, (float)particles[i].posY + addedY, addedZ);
            particlePos = new Vector3(Mathf.Clamp(particlePos.x, minOffset.x, maxOffset.x), Mathf.Clamp(particlePos.y, minOffset.y, maxOffset.y), particlePos.z); 
            if (!float.IsNaN(particlePos.x) && !float.IsNaN(particlePos.y) && !float.IsNaN(particlePos.z))
                curParticle.transform.position = particlePos;
            /*if (particles[i].particleGO.GetComponent<Rigidbody>().velocity.x > 10 && particles[i].particleGO.GetComponent<Rigidbody>().velocity.y > 10)
            {
                particles[i].particleGO.GetComponent<Rigidbody>().isKinematic = false;
            }
            else
            {
                particles[i].particleGO.GetComponent<Rigidbody>().isKinematic = true;
            }*/
            

            //ellipse.transform.localScale = new Vector3(d, d, 0);
            //ellipse((float)particles[i].posX, (float)particles[i].posY, d, d);

            Particle p = particles[i];
            if (p != null)
            {

                int cellX = (int)Mathf.Floor(p.posX / cellWidth);
                int cellY = (int)Mathf.Floor(p.posY / cellHeight);
                cellX = Mathf.Clamp(cellX, 0, numOfRows-1);
                cellY = Mathf.Clamp(cellY, 0, numOfRows-1);
                float dx = fluidSolver.getDx(cellX, cellY);
                float dy = fluidSolver.getDy(cellX, cellY);

                /*Debug.Log("DX = " + dx * vScale);
                Debug.Log("DY = " + dy * vScale);*/
                //Debug.Log("VEl = " + ellipse.GetComponent<Rigidbody>().velocity);
                if (dx * vScale < .05f && dy * vScale < .05f)
                {
                    //Debug.Log("NOT ENOUGH WIND FOR PARTICLE = " + particles[i].particleGO.name);
                    //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + " = " + new Vector2(particles[i].posX, particles[i].posY));
                    //particles[i].posY = minOffset.y;
                    //particles[i].particleGO.GetComponent<Rigidbody>().isKinematic = false;
                    //particles[i].particleGO.transform.position -= new Vector3(0, -9, 0);
                    //Debug.Log("HERE");
                    //ellipse.transform.position += new Vector3(0, -9, 0) * Time.deltaTime * 5;
                    //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + " ,SPEED = " + ellipse.transform.position);
                    //particles[i].posY = ellipse.transform.position.y - addedY;
                    //ellipse.transform.position = new Vector3(Mathf.Clamp(ellipse.transform.position.x, minOffset.x, maxOffset.x), Mathf.Clamp(ellipse.transform.position.y, minOffset.y, maxOffset.y), ellipse.transform.position.z);
                    //continue;
                    //particles[i].moving = false;
                    //particles[i].posY += -9 * Time.deltaTime * 5;
                }
                else
                {
                    //particles[i].moving = true;
                }

                float lX = p.posX - cellX * cellWidth - cellWidth / 2;
                float lY = p.posY - cellY * cellHeight - cellHeight / 2;

                cellsX.Add(cellX);
                cellsY.Add(cellY);

                int v, h, vf, hf;

                if (lX > 0)
                {
                    v = Mathf.Min(n, cellX + 1);
                    vf = 1;
                }
                else
                {
                    v = Mathf.Max(0, cellX - 1);
                    vf = -1;
                }

                if (lY > 0)
                {
                    h = Mathf.Min(n, cellY + 1);
                    hf = 1;
                }
                else
                {
                    h = Mathf.Max(0, cellY - 1);
                    hf = -1;
                }

                float dxv = fluidSolver.getDx(v, cellY);
                float dxh = fluidSolver.getDx(cellX, h);
                float dxvh = fluidSolver.getDx(v, h);

                float dyv = fluidSolver.getDy(v, cellY);
                float dyh = fluidSolver.getDy(cellX, h);
                float dyvh = fluidSolver.getDy(v, h);

                dx = Mathf.Lerp(Mathf.Lerp(dx, dxv, vf * lX / cellWidth), Mathf.Lerp(dxh, dxvh, vf * lX / cellWidth), hf * lY / cellHeight);

                dy = Mathf.Lerp(Mathf.Lerp(dy, dyv, vf * lX / cellWidth), Mathf.Lerp(dyh, dyvh, vf * lX / cellWidth), hf * lY / cellHeight);

                /*if (dx == 0 && dy == 0)
                {
                    p.particleGO.GetComponent<Rigidbody>().isKinematic = false;
                    Debug.Log("NO WIND for particle = " + p.particleGO.name);
                    continue;
                }
                else
                {
                    p.particleGO.GetComponent<Rigidbody>().isKinematic = true;
                }*/
                //Debug.Log("NO WIND for particle = " + p.particleGO.name);

                //Debug.Log(new Vector2(dx, dy));

                p.posX += dx * vScale;
                p.posY += dy * vScale;

                Vector3 pVelocity = (new Vector3(Mathf.Abs(p.posX), Mathf.Abs(p.posY), 0) - new Vector3(Mathf.Abs(p.prevPosX), Mathf.Abs(p.prevPosY), 0)) / Time.deltaTime;
                //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + " = " + pVelocity.x);
                //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + " = " + pVelocity.y);
                //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + " = " + p.posX);
                //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + ", PREV = " + p.prevPosX);

                float vectorLength = Vector3.Distance(vectorCellsOrigin[Mathf.Clamp(fluidSolver.INDEX(cellX, cellY), 0, ((numOfRows * numOfRows) - 1))], vectorCellsDir[Mathf.Clamp(fluidSolver.INDEX(cellX, cellY), 0, ((numOfRows * numOfRows) - 1))]);
                
                if (vectorLength < p.minSpeedToGravity)
                {
                    //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + " = " + vectorLength);
                    particles[i].posY += -p.gravityForce * Time.deltaTime * p.gravitySpeed;
                }
                    //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + " = " + fluidSolver.INDEX(cellX, cellY) + ", Cells = " + new Vector2(cellX, cellY));
                //Debug.Log("CELLS = "+new Vector2(cellX,cellY));


                //Velocity code
                /*if (pVelocity.x < (Vector3.one.x*3) && pVelocity.y < (Vector3.one.y*3))
                {
                    //Debug.Log("FOR PARTICLE " + particles[i].particleGO.name + " = " + pVelocity.x);
                    particles[i].posY += -9 * Time.deltaTime;
                }*/

                particles[i].prevPosY = particles[i].posY;
                particles[i].prevPosX = particles[i].posX;
                /*
                     if (p.posX < 0 || p.posX >= width) {
                       p.posX = random(width);
                     }
                     if (p.posY < 0 || p.posY >= height) {
                       p.posY = random(height);
                     }
                     */
            }
        }
    }

    private void updateParticles()
    {
        for (int i = 0; i < numParticles; i++)
        {
            Particle particle = particles[i];

            // bounce off bottom
            if (particle.posY > height - particle.radius)
            {
                particle.vY = -Mathf.Abs(particle.vY) * (1 - BOUNCE_DAMPENING);
                particle.posY = height - particle.radius;
            }

            // bounce off ceiling
            if (particle.posY < particle.radius)
            {
                particle.vY = Mathf.Abs(particle.vY) * (1 - BOUNCE_DAMPENING);
                particle.posY = particle.radius;
            }

            // bounce off left border
            if (particle.posX < particle.radius)
            {
                particle.vX = Mathf.Abs(particle.vX) * (1 - BOUNCE_DAMPENING);
                particle.posX = particle.radius;
            }

            // bounce off right border
            if (particle.posX > width - particle.radius)
            {
                particle.vX = -Mathf.Abs(particle.vX) * (1 - BOUNCE_DAMPENING);
                particle.posX = width - particle.radius;
            }

            // apply interactive gravity
            //applyMouseGravity(particle);

            // inter particle
            for (int j = i + 1; j < numParticles; j++)
            {

                // bounce
                particles[i].applyElectroStaticForce(particles[j]);
                //particles[i].bounce(particles[j]);
            }

            // apply Gravity
            if (grav)
            {
                particle.vY += FluidDynamics.GRAVITY * 0.1f * v;
            }


            // move it
            particle.tick();
        }

    }

    private void applyMouseGravity(Particle particle)
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            float d = Mathf.Sqrt(Mathf.Pow(particle.posX - mouseX, 2)
                    + Mathf.Pow(particle.posY - mouseY, 2))
                    * (float)2.0;
            float ang = Mathf.Atan2(particle.posX - mouseX, particle.posY - mouseY);
            float F = (float)24 * v;
            if (Input.GetMouseButton(1))
            {
                F = -F;
            }

            particle.vX += Mathf.Sin(ang) * F;
            particle.vY += Mathf.Cos(ang) * F;
        }
    }

    float prevMouseX;
    float prevMouseY;

    void setForce(NavierStokesSolver fluidSolver)
    {
        fluidSolver.InitField(NavierStokesSolver.dens_prev);
        fluidSolver.InitField(NavierStokesSolver.u_prev);
        fluidSolver.InitField(NavierStokesSolver.v_prev);

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            int x, y;

            x = ((int)mouseX * numOfRows) / (width) + 1;
            y = ((int)mouseY * numOfRows) / (height) + 1;

            if (Input.GetMouseButtonDown(1) && !Input.GetKeyDown(KeyCode.LeftShift))
            {
                //uPrev [x,y] += (mouseX - prevMouseX); 
                //vPrev [x,y] += (mouseY - prevMouseY);

                fluidSolver.setForceArea(NavierStokesSolver.u_prev, x, y, mouseX - prevMouseX, velocityBrushSize);
                fluidSolver.setForceArea(NavierStokesSolver.v_prev, x, y, mouseY - prevMouseY, velocityBrushSize);
            }
            else
            {
                //densPrev [x,y] += 5;
                int m = (int) Mathf.Floor((mouseX - prevMouseX) + (mouseY - prevMouseY));
                fluidSolver.setForceArea(NavierStokesSolver.dens_prev, x, y, fluidSolver.range(Mathf.Abs(m), 0, 2), densityBrushSize);
            }
        }

        prevMouseX = mouseX;
        prevMouseY = mouseY;

    }

    public static int pixelSize = 2;    // The size of each grid square on the screen;

    void drawField(NavierStokesSolver fluidSolver) {
        int x,y;
        float s;
        Color col;
  
        float ax, ay, az;
        float bx, by, bz;
        float nx=0, ny=0, nz=0;

        float vu, vv;

        float l;

        float lx=0, ly=0, lz=0;
        float vx=0, vy=0, vz=0;
        float hx=0, hy=0, hz=0;

        float ndoth, ndotl;
  
        float w;
        float d;

        bool showDensity = true;

        int count = 0;
    
        for (y = 1; y <= numOfRows; y++ ) {
            for ( x = 1; x <= numOfRows; x++ ) {

                d = NavierStokesSolver.dens[fluidSolver.INDEX(x, y)];
      
                /*if ( showNormals || showLighting ) {
                    //ax = 1; 
                    //ay = 0; 
                    az = d - dens[x+1][y];
                    //bx = 0; 
                    //by = 1; 
                    bz = d - dens[x][y+1];
          
                    //nx = ay*bz - az*by;
                    //ny = az*bx - ax*bz;
                    //nz = ax*by - ay*bx;  
        
                    nx = -az;
                    ny = -bz;
                    nz = 1;
        
                    //l = sqrt(nx*nx + ny*ny + nz*nz); if ( l != 0 ) { nx /= l; ny /= l; nz /= l; }
                    l = -sqrt(nx*nx + ny*ny + 1); nx /= l; ny /= l; nz /= l;
                }*/
      
                if ( showDensity ) {

                    if (d != 0.0f)
                    {
                        count++;
                        if (count > quads.Count)
                        {
                            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            quads.Add(quad);
                        }
                        GameObject curQuad = quads[count - 1];
                        curQuad.transform.position = new Vector3((x - 1) * pixelSize, (y - 1) * pixelSize, 0);
                        curQuad.transform.localScale = onePixel;
                        curQuad.GetComponent<Renderer>().material = quadMat;
                        //rect ( (x-1) * pixelSize, (y-1) * pixelSize, pixelSize, pixelSize );
                    }

                }
      
                /*if ( showVelocity ) {
                    if ( (x % lineSpacing) == 0 && (y % lineSpacing) == 0 ) {
                        noFill();
                        stroke(255,0,0);
                        vu = range(500 * u[x][y], -50,50);
                        vv = range(500 * v[x][y], -50,50);
                        line( (x-1)*pixelSize, (y-1)*pixelSize, (x-1)*pixelSize + vu, (y-1)*pixelSize + vv);
                    }
                }*/
      
                /*if ( showNormals ) {
                    if ( (x % lineSpacing) == 0 && (y % lineSpacing) == 0 ) {
                        noFill();
                        stroke(255);        
          
                        vu = range(500 * nx, -50,50);
                        vv = range(500 * ny, -50,50);
                        line( (x-1)*pixelSize, (y-1)*pixelSize, (x-1)*pixelSize + vu, (y-1)*pixelSize + vv);
                    } 
                } */     
            }    
        } 
    }

}

public class NavierStokesSolver {

    public static float[] u;
    public static float[] v;
    public static float[] u_prev;
    public static float[] v_prev;
    public static float[] dens;
    public static float[] dens_prev;
    public static bool[] bounds;
    public float visc;
    public float dt;
    public float diff;

    public static int N = 62;
    public static int gridSize = 0;
    public static int SIZE;

    public float[] tmp;

    public NavierStokesSolver(int numOfRows)
    {
        N = numOfRows;
        gridSize = N + 2;
        SIZE = (N + 2) * (N + 2);
        u = new float[SIZE];
        v = new float[SIZE];
        u_prev = new float[SIZE];
        v_prev = new float[SIZE];
        dens = new float[SIZE];
        dens_prev = new float[SIZE];
        tmp = new float[SIZE];
        bounds = new bool[SIZE];

        for (int i = 0; i < SIZE; i++)
        {
            dens_prev[i] = 0.01f;
        }

        visc = 0.005f;
        dt = 0.1f;
        diff = 1000.0001f;
    }

    public void Tick(float dt, float visc, float diff)
    {
        vel_step(u, v, u_prev, v_prev, visc, dt);
        dens_step(dens, dens_prev, u, v, diff, dt);
    }

    public int INDEX(int i, int j)
    {
        return i + (N + 2) * j;
    }

    void add_source (float[] x, float[] s, float dt)
    {
	    int i;
	    float size;
	
	    size=(N+2)*(N+2);
	
	    for ( i=0 ; i<size ; i++ ) 
	    {
		    x[i] += dt*s[i];
	    }	 
    }

    void diffuse(int b, float[] x, float[] x0, float diff, float dt)
    {
        int i, j, k;
        float a = dt * diff * N * N;
        for (k = 0; k < 20; k++)
        {
            for (i = 1; i <= N; i++)
            {
                for (j = 1; j <= N; j++)
                {
                    x[INDEX(i, j)] = (x0[INDEX(i, j)] + a
                      * (x[INDEX(i - 1, j)] + x[INDEX(i + 1, j)]
                      + x[INDEX(i, j - 1)] + x[INDEX(i, j + 1)]))
                      / (1 + 4 * a);
                }
            }
            set_bnd(b, x);
        }
    }

    void advect (int b, float[] d, float[] d0, float[] u, float[] v, float dt)
    {
        /* OLD CODE - BASIC REIMPLIMENTATION OF STAM'S CODE
        int i, j, i0, j0, i1, j1;
        float x, y, s0, t0, s1, t1, dt0;

        dt0 = dt * N;
	
	    for (i=1 ; i<=N ; i++) 
	    {
		    for (j=1 ; j<=N ; j++)
		    {
                if (bounds[INDEX(i, j)]) 
                    continue;
			    x = i-dt0*u[INDEX(i,j)]; 
			    y = j-dt0*v[INDEX(i,j)];
			
			    if(x<0.5f)
			    {
				    x=0.5f;
			    }	 
			
			    if(x>N+0.5f) 
			    {
				    x=N+0.5f; 
			    }
			
			    i0=Mathf.FloorToInt(x);
			    i1=i0+1; 
			
			    if(y<0.5f)
			    {
				    y=0.5f; 
			    }
			
			    if(y>N+0.5f) 
			    {
				    y=N+0.5f; 
			    }
			
			    j0=Mathf.FloorToInt(y);
			    j1=j0+1; 
			    s1 = x-i0; 
			    s0 = 1.0f-s1; 
			    t1 = y-j0; 
			    t0 = 1.0f-t1;
			
			    d[INDEX(i,j)] = s0*(t0*d0[INDEX(i0,j0)]+t1*d0[INDEX(i0,j1)])+s1*(t0*d0[INDEX(i1,j0)]+t1*d0[INDEX(i1,j1)]);
		    } 
	    }
	    set_bnd (b, d ); 
         */

        //NEW CODE = The eventual idea is to have a version of advect that uses the monotonic cubic interpolation
        int i, j, i0, j0, i1, j1;
	    float x, y, s0, t0, s1, t1, dt0, vx, vy, tleft,t,tnext;

        const float smallf = 0.0000001f;

	    dt0 = dt*N;

        for (i = 1; i <= N; i++)
        {
            for (j = 1; j <= N; j++)
            {

                if (bounds[INDEX(i, j)]) continue;

                tleft = dt0;
                x = i; y = j;

                while (tleft > smallf)
                {

                    //enforce boundry contraints
                    if (x < 0.5f) x = 0.5f; if (x > N + 0.5f) x = N + 0.5f;
                    if (y < 0.5f) y = 0.5f; if (y > N + 0.5f) y = N + 0.5f;


                    i0 = (int)x; i1 = i0 + 1;
                    j0 = (int)y; j1 = j0 + 1;
                    s1 = x - i0; s0 = 1 - s1; t1 = y - j0; t0 = 1 - t1;

                    vx = -(s0 * (t0 * u[INDEX(i0, j0)] + t1 * u[INDEX(i0, j1)]) +
                           s1 * (t0 * u[INDEX(i1, j0)] + t1 * u[INDEX(i1, j1)]));

                    vy = -(s0 * (t0 * v[INDEX(i0, j0)] + t1 * v[INDEX(i0, j1)]) +
                           s1 * (t0 * v[INDEX(i1, j0)] + t1 * v[INDEX(i1, j1)]));


                    float speed2 = vx * vx + vy * vy;
                    if (speed2 > smallf) tnext = .5f / Mathf.Sqrt(speed2);
                    else tnext = tleft;

                    t = tnext > tleft ? tleft : tnext;
                    tleft -= t;


                    x += t * vx;
                    y += t * vy;
                }


                if (x < 0.5f) x = 0.5f; if (x > N + 0.5f) x = N + 0.5f;
                if (y < 0.5f) y = 0.5f; if (y > N + 0.5f) y = N + 0.5f;


                i0 = (int)x; i1 = i0 + 1;
                j0 = (int)y; j1 = j0 + 1;
                s1 = x - i0; s0 = 1 - s1; t1 = y - j0; t0 = 1 - t1;

                d[INDEX(i, j)] = s0 * (t0 * d0[INDEX(i0, j0)] + t1 * d0[INDEX(i0, j1)]) +
                             s1 * (t0 * d0[INDEX(i1, j0)] + t1 * d0[INDEX(i1, j1)]);
            }
        }
	    set_bnd (b, d );
    }


    void SWAP(float[] x0, float[] x)
    {
	    float[] tmp = new float[x0.Length];
	    tmp=x0;
	    x0=x;
        x = tmp;
        
        // not longer used anyway

        /*System.arraycopy(x0, 0, tmp, 0, SIZE);
        System.arraycopy(x, 0, x0, 0, SIZE);
        System.arraycopy(tmp, 0, x, 0, SIZE);*/
    }

    void dens_step (float[] x, float[] x0, float[] u, float[] v, float diff, float dt)
    {
	    add_source (x, x0, dt);
	    SWAP (x0, x); 
	    diffuse (0, x, x0, diff, dt); 
	    SWAP (x0, x); 
	    advect (0, x, x0, u, v, dt);
    }

    void vel_step(float[] u, float[] v, float[] u0, float[] v0, float visc, float dt)
    {
        /*add_source(u, u0, dt);
        add_source(v, v0, dt);
        SWAP(u0, u);
        diffuse(1, u, u0, visc, dt);
        SWAP(v0, v);
        diffuse(2, v, v0, visc, dt);
        project(u, v, u0, v0);
        SWAP(u0, u);
        SWAP(v0, v);
        advect(1, u, u0, u0, v0, dt);
        advect(2, v, v0, u0, v0, dt);
        project(u, v, u0, v0);*/

        diffuse(1, u, u, visc, dt);
        diffuse(2, v, v, visc, dt);
        project(u, v, u0, v0);
    }


    void project (float[] u, float[] v, float[] p, float[] div)
    {
        int i, j, k;
        float h;
        h = 1.0f / N;
	
	    for (i=1;i<=N;i++)
	    {
		    for (j=1;j<=N;j++) 
		    {
			    div[INDEX(i,j)] = -0.5f*h*(u[INDEX(i+1,j)]-u[INDEX(i-1,j)]+v[INDEX(i,j+1)]-v[INDEX(i,j-1)]);
   			    p[INDEX(i,j)] = 0.0f;
		    }
	    }	
	
	    set_bnd (0, div); 
	    set_bnd (0, p);

	    for (k=0;k<20;k++)
	    {
		    for (i=1;i<=N;i++)
		    {
			    for(j=1;j<=N;j++)
			    {
				    p[INDEX(i,j)] = (div[INDEX(i,j)]+p[INDEX(i-1,j)]+p[INDEX(i+1,j)]+p[INDEX(i,j-1)]+p[INDEX(i,j+1)])/4.0f;
			    }
		    }
				
		    set_bnd (0, p );
	    }
	
	    for (i=1;i<=N;i++)
	    {
		    for (j=1;j<=N;j++)
		    {
			    u[INDEX(i,j)] -= 0.5f*(p[INDEX(i+1,j)]-p[INDEX(i-1,j)])/h;
			    v[INDEX(i,j)] -= 0.5f*(p[INDEX(i,j+1)]-p[INDEX(i,j-1)])/h;
           }
	    }
	
	    set_bnd (1, u);
	    set_bnd (2, v); 
    }


    void set_bnd (int b, float[] x)
    {
        /*int i; <-------------
	
	    for(i=1;i<=N;i++) 
	    {
		    if(b==1)
		    {
			    x[INDEX(0, i)] = -x[INDEX(1,i)];
			    x[INDEX(N+1,i)] = -x[INDEX(N,i)];
		    }	
		    else
		    {
			    x[INDEX(0, i)] = x[INDEX(1,i)];
			    x[INDEX(N+1,i)] = x[INDEX(N,i)];
		    }	
		
		    if(b==2)
		    {
			    x[INDEX(i,0 )]=  -x[INDEX(i,1)]; 
			    x[INDEX(i,N+1)]= -x[INDEX(i,N)];
		    }
		    else
		    {
			    x[INDEX(i,0 )]= x[INDEX(i,1)]; 
			    x[INDEX(i,N+1)]= x[INDEX(i,N)];
		    }
		
	    }*/

        /*for (i = 1; i <= N; i++) //shortenet code
        {
            x[INDEX(0, i)] = (b == 1) ? -x[INDEX(1, i)] : x[INDEX(1, i)];
            x[INDEX(N + 1, i)] = (b == 1) ? -x[INDEX(N, i)] : x[INDEX(N, i)];
            x[INDEX(i, 0)] = (b == 2) ? -x[INDEX(i, 1)] : x[INDEX(i, 1)];
            x[INDEX(i, N + 1)] = (b == 2) ? -x[INDEX(i, N)] : x[INDEX(i, N)];
        }*/

        /*x[INDEX(0, 0)] = 0.5f * (x[INDEX(1, 0)] + x[INDEX(0, 1)]); <---------------------------------
        x[INDEX(0, N + 1)] = 0.5f * (x[INDEX(1, N + 1)] + x[INDEX(0, N)]);
        x[INDEX(N + 1, 0)] = 0.5f * (x[INDEX(N, 0)] + x[INDEX(N + 1, 1)]);
        x[INDEX(N + 1, N + 1)] = 0.5f * (x[INDEX(N, N + 1)] + x[INDEX(N + 1, N)]);*/

        int i,j;
	    for ( i=1 ; i<=N ; i++ ) {
		    x[INDEX(0  ,i)] = b==1 ? 0 : x[INDEX(1,i)];
		    x[INDEX(N+1,i)] = b==1 ? 0 : x[INDEX(N,i)];
		    x[INDEX(i,0  )] = b==2 ? 0 : x[INDEX(i,1)];
		    x[INDEX(i,N+1)] = b==2 ? 0 : x[INDEX(i,N)];
	    }

	    for ( i=1 ; i<=N ; i++ ) { 
            for ( j=1 ; j<=N ; j++ ) { 
		        if(!bounds[INDEX(i,j)]) continue;
		        if( (b==1) || (b==2) ) {
			        x[INDEX(i,j)]=0;
		        }
		        else {
			        int count  =0;
			        float total=0;
			        if(!bounds[INDEX(i+1,j)]) { count++; total+=x[INDEX(i+1,j)]; }
			        if(!bounds[INDEX(i-1,j)]) { count++; total+=x[INDEX(i-1,j)]; }		
			        if(!bounds[INDEX(i,j+1)]) { count++; total+=x[INDEX(i,j+1)]; }
			        if(!bounds[INDEX(i,j-1)]) { count++; total+=x[INDEX(i,j-1)]; }
			        if(count != 0) 
                        total/=count;
			        x[INDEX(i,j)]=total;
		        }
            }
        }
        
    }
 

    public float getDx(int x, int y) {
        return u[INDEX(x + 1, y + 1)];
    }
 
    public float getDy(int x, int y) {
        return v[INDEX(x + 1, y + 1)];
    }
 
    public void applyForce(int cellX, int cellY, float vx, float vy) {
        cellX += 1;
        cellY += 1;
        float dx = u[INDEX(cellX, cellY)];
        float dy = v[INDEX(cellX, cellY)];
 
        u[INDEX(cellX, cellY)] = (vx != 0) ? Mathf.Lerp( vx, dx, 0.85f) : dx;
        v[INDEX(cellX, cellY)] = (vy != 0) ? Mathf.Lerp( vy, dy, 0.85f) : dy;
    }

    public float range(float f, float minf, float maxf)
    {
        return Mathf.Max(Mathf.Min(f, maxf), minf);
    }

    public void setForceArea(float[] field, int x, int y, float s, float r) {
  
        int i,j, dx, dy;
        float f;
  
        for ( i = (int)range(x-r,1,N); i <= (int)range(x+r,1,N); i++ ) {
            dx = x - i;
            for ( j = (int)range(y-r,1,N); j <= (int)range(y+r,1,N); j++ ) {
                dy = y - j;
                f = 1 - (Mathf.Sqrt(dx*dx + dy*dy) / r );
                field[INDEX(i, j)] += range(f, 0, 1) * s;
            }
        } 
  
    }

    public void InitField(float[] f)
    {
        for (int i = 0; i < SIZE; i++)
        {
            f[i] = 0.0f;
        }
    }
}

public class Particle
{

    public float posX = 0;
    public float posY = 0;
    public float prevPosX = 0;
    public float prevPosY = 0;

    public float vX = 0;
    public float vY = 0;

    public float radius;

    public float charge = 0;
    public GameObject particleGO;

    public float maxVelocity = 0;
    public float gravityForce = 9f;
    public float gravitySpeed = 9f;
    public float minSpeedToGravity = 3f;
    public Material particleMat;

    public Particle(float x, float y, float r)
    {
        posX = x;
        posY = y;
        radius = r;
    }

    public float getVelocity()
    {
        return Mathf.Sqrt(vX * vX + vY * vY);
    }

    public float getMotionDirection()
    {
        return Mathf.Atan2(vX, vY);
    }

    public void tick()
    {
        posX += vX * FluidDynamics.v;
        posY += vY * FluidDynamics.v;
    }

    public void applyElectroStaticForce(Particle theOtherParticle)
    {
        float d = Mathf.Sqrt(Mathf.Pow((float)(theOtherParticle.posX - posX), 2)
                + Mathf.Pow((float)(theOtherParticle.posY - posY), 2)) * ((float)0.8);

        float v = (float)127 / (d * d);
        v *= (charge - 0.5f) * (theOtherParticle.charge - 0.5f);

        float dx = (theOtherParticle.posX - posX);
        float dy = (theOtherParticle.posY - posY);

        dx *= v;
        dy *= v;

        vX -= dx;
        vY -= dy;
        theOtherParticle.vX += dx;
        theOtherParticle.vY += dy;
    }

    public void bounce(Particle theOtherParticle)
    {
        if (hit(theOtherParticle))
        {
            charge = (float)((charge + theOtherParticle.charge) * 0.5);
            theOtherParticle.charge = charge;
            float commonTangentAngle = Mathf.Atan2(
                    (float)(posX - theOtherParticle.posX),
                    (float)(posY - theOtherParticle.posY))
                    + Mathf.Asin(1);

            float v1 = theOtherParticle.getVelocity();
            float v2 = getVelocity();
            float w1 = theOtherParticle.getMotionDirection();
            float w2 = getMotionDirection();

            theOtherParticle.vX = Mathf.Sin(commonTangentAngle) * v1
                    * Mathf.Cos(w1 - commonTangentAngle)
                    + Mathf.Cos(commonTangentAngle) * v2
                    * Mathf.Sin(w2 - commonTangentAngle);
            theOtherParticle.vY = Mathf.Cos(commonTangentAngle) * v1
                    * Mathf.Cos(w1 - commonTangentAngle)
                    - Mathf.Sin(commonTangentAngle) * v2
                    * Mathf.Sin(w2 - commonTangentAngle);
            vX = Mathf.Sin(commonTangentAngle) * v2
                    * Mathf.Cos(w2 - commonTangentAngle)
                    + Mathf.Cos(commonTangentAngle) * v1
                    * Mathf.Sin(w1 - commonTangentAngle);
            vY = Mathf.Cos(commonTangentAngle) * v2
                    * Mathf.Cos(w2 - commonTangentAngle)
                    - Mathf.Sin(commonTangentAngle) * v1
                    * Mathf.Sin(w1 - commonTangentAngle);

            theOtherParticle.vX *= (1 - FluidDynamics.BOUNCE_DAMPENING);
            theOtherParticle.vY *= (1 - FluidDynamics.BOUNCE_DAMPENING);
            vX *= (1 - FluidDynamics.BOUNCE_DAMPENING);
            vY *= (1 - FluidDynamics.BOUNCE_DAMPENING);

        }
    }

    private bool hit(Particle theOtherParticle)
    {
        return (Mathf.Sqrt(Mathf.Pow((float)(theOtherParticle.posX - posX), 2)
                + Mathf.Pow((float)(theOtherParticle.posY - posY), 2)) < (theOtherParticle.radius + radius))
                && (Mathf.Sqrt(Mathf.Pow((float)(theOtherParticle.posX - posX), 2)
                        + Mathf.Pow((float)(theOtherParticle.posY - posY), 2)) > Mathf.Sqrt(Mathf.Pow(
                        (float)(theOtherParticle.posX
                                + theOtherParticle.vX * FluidDynamics.v - posX - vX * FluidDynamics.v), 2)
                        + Mathf.Pow((float)(theOtherParticle.posY
                                + theOtherParticle.vY * FluidDynamics.v - posY - vY * FluidDynamics.v), 2)));
    }

}

[Serializable]
public class ParticleTypes
{
    public int numOfParticles;
    //public string particleName;
    public GameObject prefabGo;
    public float radius = 1;
    public bool multiplyScale = false;
    public Material particleMat;
    public float gravityForce = 9;
    public float gravitySpeed = 1;
    public float minSpeedToGravity = 3;
    public float maxVelocity = 10;
    public bool hasTrail = false;
    public TrailTypes trailType;
    public bool isWind = false;
    public bool rotate = true;
    public bool randomRot = true;
    public float rotationSpeed = 80f;
    public Vector3 rotDirection = new Vector3(0, 0, -1);
    public bool randomColor = false;
}


[Serializable]
public class TrailTypes
{
    public Material trailMat;
    public float startWidth;
    public float endWidth;
    public float time;
}