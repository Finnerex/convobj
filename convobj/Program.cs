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

        Obj theObject = new Obj();
            
        Console.Write("Enter file name: ");
        string name = Console.ReadLine().Replace(".obj", "");
            
        Console.Write("Enter file path (not including file): "); // this kind of stuff wont have to exist eventually
        string path = Console.ReadLine();
        if (path[path.Length - 1] != '\\')
            path += '\\';

        theObject.LoadObj(path + name + ".obj");
        
        Console.WriteLine("Exporting data...");

        StringBuilder faces = ExportFaces(theObject.FaceList);
        StringBuilder vertices = ExportVertices(theObject.VertexList);

        int numFaces = theObject.FaceList.Count;
        int numVerts = theObject.VertexList.Count;
        
        
        Console.WriteLine("Writing C file...");
        
        StreamWriter cFile = new StreamWriter(name + ".c");
        cFile.WriteLine($"#include \"{name}.h\" \n");
        cFile.WriteLine($"face_t {name}_faces[{numFaces}] = {faces}; \n\nvec3_t {name}_verts[{numVerts}] = {vertices};\n");
        cFile.WriteLine($"mesh_t {name}_data = {{ {name}_faces, {name}_verts, {numFaces}, {numVerts} }};");
        cFile.Close();

        
        Console.WriteLine("Writing header file...");

        StreamWriter hFile = new StreamWriter(name + ".h");
        hFile.WriteLine($"#ifndef {name.ToUpper()}_H\n#define {name.ToUpper()}_H");
        hFile.WriteLine("#include \"object.h\" \n#include \"vector.h\" \n");
        
        hFile.WriteLine($"#define {name}_mesh &{name}_data");
        hFile.WriteLine($"extern mesh_t {name}_data;\n");
        
        hFile.WriteLine("#endif");
        hFile.Close();
        
        Console.WriteLine("Done!");

    }

    private static StringBuilder ExportVertices(List<Vertex> vertexList)
    {
        StringBuilder vertices = new StringBuilder("{\n"); // begin vertices

        for (int i = 0; i < vertexList.Count; i++)
        {
            Vertex vert = vertexList[i];

            vertices.Append($"{{ {(float)vert.X:F1}f, {(float)vert.Y:F1}f, {(float)vert.Z:F1}f }}");

            if (i < vertexList.Count - 1)
                vertices.Append(", ");
                
            if (i % 6 == 5 && i < vertexList.Count - 1)
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
                
            if (i % 8 == 7 && i < faceList.Count - 1)
                faces.Append("\n");

        }

        return faces.Append("\n}"); // end faces
    }
        
}