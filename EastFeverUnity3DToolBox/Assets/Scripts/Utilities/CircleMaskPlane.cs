using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 2D 플레인 메쉬를 대상으로 원형 마스크를 적용해서,
// 플레인의 텍스쳐가 Pie모양으로 채워지거나 가려지도록 한다.
[ExecuteInEditMode, RequireComponent( typeof( MeshRenderer ) )]
public class CircleMaskPlane : MonoBehaviour
{    
    // 플레인의 텍스쳐 비율( 0.0 ~ 1.0f )
    public float FillRatio = 1.0f;

    // 플레인에 텍스쳐를 채우는 방향( 시계방향 / 반시계 방향 )
    public bool ClockWiseFill = true;
    
    private float _previousMaskRatio = 1.0f;

    /// X = left, Y = bottom, Z = right, W = top.
    private readonly Vector4 _planePosition = new Vector4( -1.0f, -1.0f, 1.0f, 1.0f );
    private readonly Vector4 _planeUVs = new Vector4( 0.0f, 0.0f, 1.0f, 1.0f );
    private readonly int _planeIndexArraySize = 36;

    private List<Vector3> _planeVertexList = new List<Vector3>();
    private List<Vector2> _planeUVList = new List<Vector2>();
    private int[] _planeIndexArray = null;

    private Vector2[] _tempPosition = new Vector2[ 4 ];
    private Vector2[] _tempUV = new Vector2[ 4 ];

    private MeshFilter _meshFilter = null;
    private Mesh _mesh = null;

	// Use this for initialization
	void Awake () 
    {
        _planeIndexArray = new int[ _planeIndexArraySize ];
        _meshFilter = gameObject.GetComponent<MeshFilter>();
        
        _mesh = new Mesh();
        _mesh.hideFlags = HideFlags.DontSave;
        _mesh.name = "CircleMaskPlane";
        _mesh.MarkDynamic();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if( FillRatio > 1.0f )
        {
            FillRatio = 1.0f;
        }
        if( FillRatio < 0.0f )
        {
            FillRatio = 0.0f;
        }

        if( _previousMaskRatio != FillRatio )
        {
            _previousMaskRatio = FillRatio;
            this.RefreshMask();
        }
	}

    private void RefreshMask()
    {
        _planeVertexList.Clear();
        _planeUVList.Clear();
        _mesh.Clear();

        if( FillRatio < 0.001f )
        {
            return;
        }

        Vector4 v = _planePosition;
        Vector4 u = _planeUVs;

        for( int corner = 0; corner < 4; ++corner )
        {
            float fx0, fx1, fy0, fy1;

            if( corner < 2 ) { fx0 = 0f; fx1 = 0.5f; }
            else { fx0 = 0.5f; fx1 = 1f; }

            if( corner == 0 || corner == 3 ) { fy0 = 0f; fy1 = 0.5f; }
            else { fy0 = 0.5f; fy1 = 1f; }

            _tempPosition[ 0 ].x = Mathf.Lerp( v.x, v.z, fx0 );
            _tempPosition[ 1 ].x = _tempPosition[ 0 ].x;
            _tempPosition[ 2 ].x = Mathf.Lerp( v.x, v.z, fx1 );
            _tempPosition[ 3 ].x = _tempPosition[ 2 ].x;

            _tempPosition[ 0 ].y = Mathf.Lerp( v.y, v.w, fy0 );
            _tempPosition[ 1 ].y = Mathf.Lerp( v.y, v.w, fy1 );
            _tempPosition[ 2 ].y = _tempPosition[ 1 ].y;
            _tempPosition[ 3 ].y = _tempPosition[ 0 ].y;

            _tempUV[ 0 ].x = Mathf.Lerp( u.x, u.z, fx0 );
            _tempUV[ 1 ].x = _tempUV[ 0 ].x;
            _tempUV[ 2 ].x = Mathf.Lerp( u.x, u.z, fx1 );
            _tempUV[ 3 ].x = _tempUV[ 2 ].x;

            _tempUV[ 0 ].y = Mathf.Lerp( u.y, u.w, fy0 );
            _tempUV[ 1 ].y = Mathf.Lerp( u.y, u.w, fy1 );
            _tempUV[ 2 ].y = _tempUV[ 1 ].y;
            _tempUV[ 3 ].y = _tempUV[ 0 ].y;

            float val = 0.0f;
            if( ClockWiseFill )
            {
                val = FillRatio * 4f - this.RepeatIndex( corner + 2, 4 );
            }
            else
            {
                val = FillRatio * 4f - ( 3 - this.RepeatIndex( corner + 2, 4 ) );
            }

            if( RadialCut( _tempPosition, _tempUV, Mathf.Clamp01( val ), ClockWiseFill, this.RepeatIndex( corner + 2, 4 ) ) )
            {
                for( int i = 0; i < 4; ++i )
                {
                    _planeVertexList.Add( _tempPosition[ i ] );
                    _planeUVList.Add( _tempUV[ i ] );                    
                }
            }            
        }

        this.RefreshIndexBuffer( _planeVertexList.Count );
        
        _mesh.vertices = _planeVertexList.ToArray();
        _mesh.uv = _planeUVList.ToArray();
        _mesh.triangles = _planeIndexArray;

        _meshFilter.mesh = _mesh;

    }

    private int RepeatIndex( int val, int max )
    {
        if( max < 1 ) return 0;
        while( val < 0 ) val += max;
        while( val >= max ) val -= max;
        return val;
    }

    private bool RadialCut( Vector2[] xy, Vector2[] uv, float fill, bool invert, int corner )
    {
        // Nothing to fill
        if( fill < 0.001f ) return false;

        // Even corners invert the fill direction
        if( ( corner & 1 ) == 1 ) invert = !invert;

        // Nothing to adjust
        if( !invert && fill > 0.999f ) return true;

        // Convert 0-1 value into 0 to 90 degrees angle in radians
        float angle = Mathf.Clamp01( fill );
        if( invert ) angle = 1f - angle;
        angle *= 90f * Mathf.Deg2Rad;

        // Calculate the effective X and Y factors
        float cos = Mathf.Cos( angle );
        float sin = Mathf.Sin( angle );

        RadialCut( xy, cos, sin, invert, corner );
        RadialCut( uv, cos, sin, invert, corner );
        return true;
    }

    /// <summary>
    /// Adjust the specified quad, making it be radially filled instead.
    /// </summary>

    private void RadialCut( Vector2[] xy, float cos, float sin, bool invert, int corner )
    {
        int i0 = corner;
        int i1 = RepeatIndex( corner + 1, 4 );
        int i2 = RepeatIndex( corner + 2, 4 );
        int i3 = RepeatIndex( corner + 3, 4 );

        if( ( corner & 1 ) == 1 )
        {
            if( sin > cos )
            {
                cos /= sin;
                sin = 1f;

                if( invert )
                {
                    xy[ i1 ].x = Mathf.Lerp( xy[ i0 ].x, xy[ i2 ].x, cos );
                    xy[ i2 ].x = xy[ i1 ].x;
                }
            }
            else if( cos > sin )
            {
                sin /= cos;
                cos = 1f;

                if( !invert )
                {
                    xy[ i2 ].y = Mathf.Lerp( xy[ i0 ].y, xy[ i2 ].y, sin );
                    xy[ i3 ].y = xy[ i2 ].y;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }

            if( !invert ) xy[ i3 ].x = Mathf.Lerp( xy[ i0 ].x, xy[ i2 ].x, cos );
            else xy[ i1 ].y = Mathf.Lerp( xy[ i0 ].y, xy[ i2 ].y, sin );
        }
        else
        {
            if( cos > sin )
            {
                sin /= cos;
                cos = 1f;

                if( !invert )
                {
                    xy[ i1 ].y = Mathf.Lerp( xy[ i0 ].y, xy[ i2 ].y, sin );
                    xy[ i2 ].y = xy[ i1 ].y;
                }
            }
            else if( sin > cos )
            {
                cos /= sin;
                sin = 1f;

                if( invert )
                {
                    xy[ i2 ].x = Mathf.Lerp( xy[ i0 ].x, xy[ i2 ].x, cos );
                    xy[ i3 ].x = xy[ i2 ].x;
                }
            }
            else
            {
                cos = 1f;
                sin = 1f;
            }

            if( invert ) xy[ i3 ].y = Mathf.Lerp( xy[ i0 ].y, xy[ i2 ].y, sin );
            else xy[ i1 ].x = Mathf.Lerp( xy[ i0 ].x, xy[ i2 ].x, cos );
        }
    }

    void RefreshIndexBuffer( int vertexCount )
    {

        for( int i = 0; i < _planeIndexArraySize; i++ )
        {
            _planeIndexArray[ i ] = 0;
        }

        int index = 0;
        for( int i = 0; i < vertexCount; i += 4 )
        {
            _planeIndexArray[ index++ ] = i;
            _planeIndexArray[ index++ ] = i + 1;
            _planeIndexArray[ index++ ] = i + 2;

            _planeIndexArray[ index++ ] = i + 2;
            _planeIndexArray[ index++ ] = i + 3;
            _planeIndexArray[ index++ ] = i;
        }        
    }    
}
