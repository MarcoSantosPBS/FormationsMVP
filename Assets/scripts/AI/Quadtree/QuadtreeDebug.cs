using System.Collections.Generic;
using UnityEngine;

public class QuadtreeDebug : MonoBehaviour
{
    private Quadtree<GameObject> Quadtree;
    public int capacidade = 4; // M�ximo de unidades por n� antes de subdividir
    public Rect area = new Rect(-50, -50, 150, 150); // �rea total da Quadtree
    public GameObject unidadePrefab; // Prefab para representar uma unidade no Unity
    private List<GameObject> unidadesVisuais = new List<GameObject>(); // Lista dos objetos das unidades

    public Rect regiaoDeBusca = new Rect(30, 30, 40, 40); // �rea de busca
    private List<GameObject> unidadesSelecionadas = new List<GameObject>(); // Lista de objetos encontrados na busca

    void Start()
    {
        Quadtree = new Quadtree<GameObject>(capacidade, area);

        // Criando unidades e inserindo na Quadtree
        for (int i = 0; i < 100; i++)
        {
            Vector2Int pos = new Vector2Int(Random.Range(0, 100), Random.Range(0, 100));

            // Criar objeto visual para representar a unidade
            GameObject unidade = Instantiate(unidadePrefab, new Vector3(pos.x, 0, pos.y), Quaternion.identity);
            unidade.GetComponent<Renderer>().material.color = Color.white; // Cor padr�o
            unidadesVisuais.Add(unidade);
            Quadtree.Insert(new Rect(pos, new Vector2(1,1)), unidade);
        }
    }

    private void Update()
    {
        // Buscar unidades na regi�o definida
        List<GameObject> resultados = Quadtree.Search(regiaoDeBusca);

        // Mudar a cor das unidades encontradas
        foreach (GameObject pos in resultados)
        {
            pos.GetComponent<Renderer>().material.color = Color.red; // Mudar cor para vermelho
            unidadesSelecionadas.Add(pos);
        }
    }

    // Desenhar visualmente a �rea de busca e a Quadtree no Gizmos
    void OnDrawGizmos()
    {
        if (Quadtree != null)
        {
            DrawQuadtree(Quadtree);
        }

        // Desenhar a �rea de busca em azul
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(new Vector3(regiaoDeBusca.x + regiaoDeBusca.width / 2, 0, regiaoDeBusca.y + regiaoDeBusca.height / 2),
                            new Vector3(regiaoDeBusca.width, 0.1f, regiaoDeBusca.height));
    }

    // Fun��o recursiva para desenhar a Quadtree no Gizmos
    void DrawQuadtree(Quadtree<GameObject> qt)
    {
        if (qt == null) return;

        // Desenhar os bounds da regi�o da Quadtree
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(qt.bound.x + qt.bound.width / 2, 0, qt.bound.y + qt.bound.height / 2),
                            new Vector3(qt.bound.width, 0.1f, qt.bound.height));

        // Se a Quadtree foi subdividida, desenhar os quadrantes
        if (qt.wasSubdivided)
        {
            DrawQuadtree(qt.nordeste);
            DrawQuadtree(qt.noroeste);
            DrawQuadtree(qt.sudeste);
            DrawQuadtree(qt.sudoeste);
        }
    }
}
