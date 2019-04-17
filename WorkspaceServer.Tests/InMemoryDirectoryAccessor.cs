﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Try.Markdown;

namespace WorkspaceServer.Tests
{
    public class InMemoryDirectoryAccessor : IDirectoryAccessor, IEnumerable
    {
        private readonly DirectoryInfo _rootDirToAddFiles;
        private Dictionary<string, string> _files;

        public InMemoryDirectoryAccessor(
            DirectoryInfo workingDirectory = null,
            DirectoryInfo rootDirectoryToAddFiles = null)
        {
            WorkingDirectory = workingDirectory ??
                               new DirectoryInfo(Directory.GetCurrentDirectory());
            _rootDirToAddFiles = rootDirectoryToAddFiles ??
                                 WorkingDirectory;
            _files = new Dictionary<string, string>();
        }

        public DirectoryInfo WorkingDirectory { get; }

        public void Add((string path, string content) file)
        {
            _files.Add(
                new FileInfo(Path.Combine(_rootDirToAddFiles.FullName, file.path)).FullName, file.content);
        }

        public bool FileExists(RelativeFilePath filePath)
        {
            return _files.ContainsKey(GetFullyQualifiedPath(filePath).FullName);
        }

        public string ReadAllText(RelativeFilePath filePath)
        {
            _files.TryGetValue(GetFullyQualifiedPath(filePath).FullName, out var value);
            return value;
        }

        public FileSystemInfo GetFullyQualifiedPath(RelativePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException();
            }

            switch (path)
            {
                case RelativeFilePath rfp:
                    return WorkingDirectory.Combine(rfp);
                case RelativeDirectoryPath rdp:
                    return WorkingDirectory.Combine(rdp);
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IDirectoryAccessor GetDirectoryAccessorForRelativePath(RelativeDirectoryPath relativePath)
        {
            var newPath = WorkingDirectory.Combine(relativePath);
            return new InMemoryDirectoryAccessor(newPath)
                   {
                       _files = _files
                   };
        }

        public IEnumerable<RelativeFilePath> GetAllFilesRecursively()
        {
            return _files.Keys.Select(key => new RelativeFilePath(
                                          Path.GetRelativePath(WorkingDirectory.FullName, key)));
        }
    }
}