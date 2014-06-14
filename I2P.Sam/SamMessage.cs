using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace I2P.Sam
{
	/// <summary>
	/// Represents a SAM bridge message.
	/// </summary>
	public sealed class SamMessage
	{
		private readonly bool isModifiable = false;
		private readonly NameValueCollection args = new NameValueCollection();

		/// <summary>
		/// Creates a new SAM message with the given module and operation.
		/// </summary>
		/// <param name="module">SAM bridge module.</param>
		/// <param name="operation">Module operation.</param>
		public SamMessage(string module, string operation)
		{
			this.Module = module.ToUpper();
			this.Operation = operation.ToUpper();
			this.isModifiable = true;
		}

		/// <summary>
		/// Parses a SAM message from a given string.
		/// </summary>
		/// <param name="line"></param>
		public SamMessage(string line)
		{
			this.isModifiable = false;
			foreach (var item in this.Parse(line))
			{
				if (item.Value != null)
				{
					this.args[item.Key.ToUpper()] = item.Value;
				}
				else
				{
					if (this.Module == null)
					{
						this.Module = item.Key;
					}
					else if (this.Operation == null)
					{
						this.Operation = item.Key;
					}
					else
					{
						throw new InvalidMessageException("The message contains more than two base informations.");
					}
				}
			}
		}

		/// <summary>
		/// Parses key value pairs from the response line.
		/// </summary>
		/// <param name="line">Response line from SAM.</param>
		/// <returns>Enumeration of key value pairs.</returns>
		/// <remarks>If result.Value == null, then the pair is only a single item.</remarks>
		private IEnumerable<KeyValuePair<string, string>> Parse(string line)
		{
			while (line.Length > 0)
			{
				string part = line;
				int idxSeparator = line.IndexOf(' ');
				if (idxSeparator >= 0)
				{
					part = line.Substring(0, idxSeparator);
				}

				int idxEq = part.IndexOf('=');
				if (idxEq >= 0)
				{
					string first = part.Substring(0, idxEq);
					string second = part.Substring(idxEq + 1);
					yield return new KeyValuePair<string, string>(first, second);
				}
				else
				{
					yield return new KeyValuePair<string, string>(part, null);
				}

				// Jump out if end
				if (line.Length <= part.Length)
				{
					yield break;
				}
				line = line.Substring(part.Length + 1);
			}
		}

		/// <summary>
		/// Validates a message.
		/// </summary>
		/// <param name="module">The module the message should have.</param>
		/// <param name="operation"The operation the message should have.></param>
		/// <param name="values">A collection of key-value-pairs that represent message arguments.</param>
		/// <returns>if the message is valid or not.</returns>
		public bool Validate(string module, string operation, params string[][] arguments)
		{
			if (this.Module != module.ToUpper())
			{
				return false;
			}
			if (this.Operation != operation.ToUpper())
			{
				return false;
			}

			foreach (var arg in arguments)
			{
				if (arg.Length != 1 && arg.Length != 2)
				{
					throw new ArgumentException("Every array in arguments must be at of length 1 or 2.", "arguments");
				}
				// Only check for existance
				if(arg.Length == 1)
				{
					if (this[arg[0].ToUpper()] == null)
					{
						return false;
					}
				}
				else
				{
					if (this[arg[0].ToUpper()] != arg[1])
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Gets or sets an argument of the message.
		/// </summary>
		/// <param name="arg">Name of the argument.</param>
		/// <returns>Value of the argument.</returns>
		/// <remarks>Uppercasing is done by this indexer. No need to explicitly uppercase arg.</remarks>
		public string this[string arg]
		{
			get { return this.args[arg.ToUpper()]; }
			set
			{
				if (!this.isModifiable)
					throw new InvalidOperationException("Cannot modify a parsed message.");
				this.args[arg.ToUpper()] = value;
			}
		}

		/// <summary>
		/// Gets or sets the message module.
		/// </summary>
		public string Module { get; private set; }

		/// <summary>
		/// Gets or sets the message operation.
		/// </summary>
		public string Operation { get; private set; }

		/// <summary>
		/// Gets a value that indicates whether this message is modifiable or not.
		/// Received or parsed messages are not modifiable, newly created ones are.
		/// </summary>
		public bool IsModifiable
		{
			get { return isModifiable; }
		}

		/// <summary>
		/// Returns a string representation of the message.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder(512);
			builder.AppendFormat(
				"{0} {1}",
				(this.Module ?? "<null>").ToUpper(),
				(this.Operation ?? "<null>").ToUpper());

			foreach (var key in this.args.AllKeys)
			{
				builder.AppendFormat(" {0}=", key);
				var value = this.args[key];
				if (value.Contains(' '))
					builder.AppendFormat("\"{0}\")", value);
				else
					builder.Append(value);
			}

			return builder.ToString();
		}
	}
}
