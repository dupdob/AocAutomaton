// MIT License
// 
//  AocAutomaton
// 
//  Copyright (c) 2023 Cyrille DUPUYDAUBY
// ---
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;

namespace AoC;
/// <summary>
/// Implements a factory logic for (AoC) solver instances, either from a type or an existing instance.
/// Provide caching logic to support reuse across questions.
/// </summary>
public class SolverFactory
{
    private readonly Dictionary<string, Dictionary<string, ISolver>> _solvers = new();
    private readonly Func<string, ISolver> _builder;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="builder">lambda returning an ISolver implementation</param>
    public SolverFactory(Func<string, ISolver> builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// Returns a <see cref="SolverFactory"/> instance that provide solver of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">A type implementing <see cref="ISolver"/></typeparam>
    /// <returns>A <see cref="SolverFactory"/> instance.</returns>
    public static SolverFactory ForType<T>() where T:ISolver => ForType(typeof(T));

    /// <summary>
    /// Returns a <see cref="SolverFactory"/> instance that provide solver of the same type than the instance provided..
    /// </summary>
    /// <param name="instance">A <see cref="ISolver"/> implementation.</param>
    /// <returns>A <see cref="SolverFactory"/> instance.</returns>
    public static SolverFactory From(ISolver instance) => ForType(instance.GetType());

    /// <summary>
    /// Returns a <see cref="SolverFactory"/> instance that provide solver of type <paramref name="solverType"/>.
    /// </summary>
    /// <param name="solverType">A type implementing <see cref="ISolver"/></param>
    /// <returns>A <see cref="SolverFactory"/> instance.</returns>
    public static SolverFactory ForType(Type solverType)
    {
        var constructorInfo = solverType.GetConstructor(new []{typeof(string)});

        if (constructorInfo != null)
        {
            return new SolverFactory(
                (data) => constructorInfo.Invoke(new object[]{data}) as ISolver ??
                       throw new Exception($"Can't build an instance of {solverType.Name}."));            
        }
        constructorInfo = solverType.GetConstructor(Type.EmptyTypes);
        
        if (constructorInfo == null)
        {
            throw new ApplicationException($"Can't find a parameterless constructor for {solverType.Name}.");
        }

        return new SolverFactory(
            (_) => constructorInfo.Invoke(null) as ISolver ??
                  throw new Exception($"Can't build an instance of {solverType.Name}."));
    }
    
    /// <summary>
    /// Get or set the cache status (default true)
    /// </summary>
    public bool CacheActive { get; set; } = true;

    /// <summary>
    /// Returns an <see cref="ISolver"/> implementation for the provided input data.
    /// </summary>
    /// <param name="data">input data to be used by the solver.</param>
    /// <param name="init">initial data for the solver (optional)</param>
    /// <returns>the appropriate <see cref="ISolver"/> implementation.</returns>
    /// <remarks>build a new instance or return a previously build instance associated with the same data (if <see cref="CacheActive"/> is true).</remarks>
    public ISolver GetSolver(string data, string init = null)
    {
        init ??= string.Empty;
        if (!CacheActive)
        {
            return _builder(init);
        }
        
        if (!_solvers.TryGetValue(init, out var solvers))
        {
            solvers = new Dictionary<string, ISolver>();
            _solvers[init] = solvers;
        }

        if (solvers.TryGetValue(data, out var solver))
        {
            return solver;
        }
        solver = _builder(init);
        solvers[data] = solver;
        
        return solver;
    }
}