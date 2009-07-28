// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;

namespace Remotion.Data.Linq.Clauses.StreamedData
{
  /// <summary>
  /// Holds the data needed to represent the output or input of a part of a query in memory. This is mainly used for 
  /// <see cref="ResultOperatorBase.ExecuteInMemory"/>.  The data is a single, non-sequence value and can only be consumed by result operators 
  /// working with single values.
  /// </summary>
  public class StreamedValue : IStreamedData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamedValue"/> class, setting the <see cref="Value"/> property.
    /// </summary>
    /// <param name="currentValue">The current value or sequence.</param>
    public StreamedValue (object currentValue)
    {
      Value = currentValue;
      DataInfo = new StreamedValueInfo (currentValue != null ? currentValue.GetType () : typeof (object));
    }

    /// <summary>
    /// Gets an object describing the data held by this <see cref="StreamedValue"/> instance.
    /// </summary>
    /// <value>
    /// An <see cref="StreamedValueInfo"/> object describing the data held by this <see cref="StreamedValue"/> instance.
    /// </value>
    public StreamedValueInfo DataInfo { get; private set; }

    /// <summary>
    /// Gets the current value for the <see cref="ResultOperatorBase.ExecuteInMemory(IStreamedData)"/> operation. If the object is used as input, this 
    /// holds the input value for the operation. If the object is used as output, this holds the result of the operation.
    /// </summary>
    /// <value>The current value.</value>
    public object Value { get; private set; }

    IStreamedDataInfo IStreamedData.DataInfo 
    { 
      get { return DataInfo; } 
    }

    /// <summary>
    /// Gets the value held by <see cref="Value"/>, throwing an exception if the value is not of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of the value.</typeparam>
    /// <returns><see cref="Value"/>, cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Value"/> if not of the expected type.</exception>
    public T GetTypedValue<T> ()
    {
      try
      {
        return (T) Value;
      }
      catch (InvalidCastException ex)
      {
        string message = string.Format (
            "Cannot retrieve the current value as type '{0}' because it is of type '{1}'.",
            typeof (T).FullName,
            Value.GetType ().FullName);
        throw new InvalidOperationException (message, ex);
      }
      catch (NullReferenceException ex)
      {
        string message = string.Format ("Cannot retrieve the current value as type '{0}' because it is null.", typeof (T).FullName);
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}