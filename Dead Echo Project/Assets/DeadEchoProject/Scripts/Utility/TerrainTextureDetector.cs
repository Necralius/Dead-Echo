using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class TerrainTextureDetector
{
    private Transform   _interactorTransform    = null;
    private Terrain     _terrainObject          = null;
    private Collider    _collider               = null;

    private int posX = 0;
    private int posZ = 0;

    private bool isGrounded    = false;
    private bool isOnTerrain   = false;

    public List<TextureArea> _textureAreas = new List<TextureArea>();

    public TerrainTextureDetector(GameObject objectOrigin, Collider collider)
    {
        _interactorTransform = objectOrigin.transform;
        _collider = collider;
    }

    private void UpdateData()
    {
        if (Physics.Raycast(_interactorTransform.position, Vector3.down, out RaycastHit hit, _collider.bounds.extents.y + 0.5f))
        {
            isGrounded = true;
            if (hit.transform.CompareTag("Terrain"))
            {
                _terrainObject = hit.transform.gameObject.GetComponent<Terrain>();
                isOnTerrain = true;

                _textureAreas.Clear();

                foreach (var layer in _terrainObject.terrainData.terrainLayers)
                    _textureAreas.Add(new TextureArea(layer.name, 0));
            }
            else isOnTerrain = false;
        }
        else
        {
            isGrounded      = false;
            isOnTerrain     = false;
        }
    }

    private void UpdatePosition()
    {
        UpdateData();

        if (!isOnTerrain)           return;
        if (_terrainObject == null) return;

        Vector3 terrainPosition = _interactorTransform.position - _terrainObject.transform.position;
        Vector3 mapPosition     = new Vector3(terrainPosition.x / _terrainObject.terrainData.size.x, 0, terrainPosition.z / _terrainObject.terrainData.size.z);

        float xCoord = mapPosition.x * _terrainObject.terrainData.alphamapWidth;
        float zCoord = mapPosition.z * _terrainObject.terrainData.alphamapHeight;

        posX = (int)xCoord;
        posZ = (int)zCoord;

        float[,,] splatMap  = _terrainObject.terrainData.GetAlphamaps(posX, posZ, 1, 1);

        int textureCount    = _terrainObject.terrainData.terrainLayers.Length;

        for (int i = 0; i < textureCount; i++)
        {
            string layerName = _terrainObject.terrainData.terrainLayers[i].name;
     
            _textureAreas[i] = new TextureArea(layerName, splatMap[0, 0, i]);
        }
    }

    public string GetCurrentTexture()
    {
        UpdatePosition();
        if (!isOnTerrain)           return "None";
        if (_terrainObject == null) return "None";

        string greaterLayer = _textureAreas[0].textureLayerName;
        float greaterLayerValue = 0f;

        for (int i = 0; i < _textureAreas.Count; i++)
        {
            if (_textureAreas[i].textureValue > greaterLayerValue) 
                greaterLayer = _textureAreas[i].textureLayerName;
        }

        return greaterLayer;
    }

    [Serializable]
    public struct TextureArea
    {
        public string   textureLayerName;
        public float    textureValue;

        public TextureArea(string name, float value)
        {
            textureLayerName    = name;
            textureValue        = value;
        }
    }
}