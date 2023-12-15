using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using ObjParser;
using ObjParser.Types;

namespace convobj;

internal class Program
{
    public static void Main(string[] args)
    {

        Console.Write("Enter project objs folder path (hit enter for last path): ");
        string path = Console.ReadLine();

        if (path == "")
            path = new StreamReader("cachedPath.txt").ReadLine();
        
        else
        {
            if (path[path.Length - 1] != '\\')
                path += '\\';

            StreamWriter cache = new StreamWriter("cachedPath.txt");
            cache.WriteLine(path);
            cache.Close();
        }
        

        StreamReader converts = new StreamReader(path + "converts.txt");
        
        StreamWriter objsH = new StreamWriter(path + "objs.h");
        objsH.WriteLine("#ifndef OBJS_H\n#define OBJS_H\n");

        
        string name = converts.ReadLine();

        while (name != null)
        {
            name = name.Replace(".obj", "");
            
            objsH.WriteLine($"#include \"{name}.h\"");
            
            Obj theObject = new Obj();
    
            theObject.LoadObj(path + name + ".obj");
            
            Console.WriteLine("Exporting data ...");
    
            string faces = ExportFaces(theObject.FaceList).ToString();
            string vertices = ExportVertices(theObject.VertexList).ToString();
            
            WriteCFile(name, path, theObject.FaceList.Count, theObject.VertexList.Count, faces, vertices);
            WriteHFile(name, path);

            name = converts.ReadLine();
        }

        objsH.WriteLine("\n#endif");
        objsH.Close();
        
        Console.WriteLine("Done!");

    }

    private static void WriteCFile(string name, string path, int numFaces, int numVerts, string faces, string vertices)
    {
        Console.WriteLine($"Writing {name}.c ...");

        StreamWriter cFile = new StreamWriter(path + name + ".c");
        cFile.WriteLine($"#include \"{name}.h\" \n");
        cFile.WriteLine($"face_t {name}_faces[{numFaces}] = {faces}; \n\nvec3_t {name}_verts[{numVerts}] = {vertices};\n");
        cFile.WriteLine($"mesh_t {name}_data = {{ {name}_faces, {name}_verts, {numFaces}, {numVerts} }};");
        cFile.Close();
    }
    
    private static void WriteHFile(string name, string path)
    {
        Console.WriteLine($"Writing {name}.h ...");

        StreamWriter hFile = new StreamWriter(path + name + ".h");
        hFile.WriteLine($"#ifndef {name.ToUpper()}_H\n#define {name.ToUpper()}_H");
        hFile.WriteLine("#include \"../object.h\" \n#include \"../vector.h\" \n"); // maybe theres a way of not doing ../
        
        hFile.WriteLine($"#define {name}_mesh &{name}_data");
        hFile.WriteLine($"extern mesh_t {name}_data;\n");
        
        hFile.WriteLine("#endif");
        hFile.Close();
    }

    private static StringBuilder ExportVertices(List<Vertex> vertexList)
    {
        StringBuilder vertices = new StringBuilder("{\n"); // begin vertices

        for (int i = 0; i < vertexList.Count; i++)
        {
            Vertex vert = vertexList[i];

            vertices.Append($"{{ {(float)vert.X}, {(float)vert.Y}, {(float)vert.Z} }}"); // f you

            if (i < vertexList.Count - 1)
                vertices.Append(", ");
                
            if (i % 4 == 3 && i < vertexList.Count - 1)
                vertices.Append("\n");

        }

        return vertices.Append("\n}");
    }

    private static StringBuilder ExportFaces(List<Face> faceList)
    {
        StringBuilder faces = new StringBuilder("{\n"); // begin faces

        for (int i = 0; i < faceList.Count; i++)
        {
            Face face = faceList[i];
                
            if (face.VertexIndexList.Length > 3)
            {
                Console.WriteLine("Face has more than 3 vertices. (You can fix this in blender edit mode by hitting 'a' then 'ctrl+t')");
                throw new Exception("Idk Man");
            }

            int[] vertIndices = face.VertexIndexList;
            faces.Append($"{{ {vertIndices[0] - 1}, {vertIndices[1] - 1}, {vertIndices[2] - 1} }}"); // me when index starts at 1!!!!!!!!

            if (i < faceList.Count - 1)
                faces.Append(", ");
                
            if (i % 6 == 5 && i < faceList.Count - 1)
                faces.Append("\n");

        }

        return faces.Append("\n}"); // end faces
    }
        
}