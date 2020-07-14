// ReSharper disable CheckNamespace
#pragma warning disable 1591

using GSF;
using GSF.ComponentModel;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data.Model;
using GSF.Reflection;
using GSF.TimeSeries.Adapters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace openHistorian.Model
{
    public class CustomActionAdapter
    {
        #region [ Fields ] 
        
        private List<ConnectionStringParameter> m_parameters;

        #endregion
        #region [ Properties ]
        // TODO: This expression fails to evaluate, even when Global is defined
        //[DefaultValueExpression("Global.NodeID")]
        public Guid NodeID { get; set; }

        [PrimaryKey(true)]
        public int ID { get; set; }

        [Required]
        [StringLength(200)]
        [AcronymValidation]
        public string AdapterName { get; set; }

        [Required]
        [DefaultValue("DynamicCalculator.dll")]
        public string AssemblyName { get; set; }

        [Required]
        [DefaultValue("DynamicCalculator.DynamicCalculator")]
        public string TypeName { get; set; }

        public string ConnectionString { get; set; }

        public int LoadOrder { get; set; }

        public bool Enabled { get; set; }

        [DefaultValueExpression("DateTime.UtcNow")]
        public DateTime CreatedOn { get; set; }

        [Required]
        [StringLength(200)]
        [DefaultValueExpression("UserInfo.CurrentUserID")]
        public string CreatedBy { get; set; }

        [DefaultValueExpression("this.CreatedOn", EvaluationOrder = 1)]
        [UpdateValueExpression("DateTime.UtcNow")]
        public DateTime UpdatedOn { get; set; }

        [Required]
        [StringLength(200)]
        [DefaultValueExpression("this.CreatedBy", EvaluationOrder = 1)]
        [UpdateValueExpression("UserInfo.CurrentUserID")]
        public string UpdatedBy { get; set; }

        [NonRecordField]
        public List<ConnectionStringParameter> ConnectionStringParameters
        {
            get
            {
                if (m_parameters == null)
                    m_parameters = GetConnectionStringParameters();
                return m_parameters;
            }
            set {
                    m_parameters = value;
            }
        }

        #endregion

        #region Methods

        private List<ConnectionStringParameter> GetConnectionStringParameters()
        {
            if (this.AssemblyName != null && this.TypeName != null)
            {
                Attribute connectionStringParameterAttribute;

                Type adapterType = null;

                // Attempt to find that assembly and retrieve the type.
                if (File.Exists(this.AssemblyName))
                    adapterType = Assembly.LoadFrom(this.AssemblyName).GetType(this.TypeName);

                if (adapterType != null)
                {
                    // Get the list of properties with ConnectionStringParameterAttribute annotations.
                    IEnumerable<PropertyInfo> infoList = adapterType.GetProperties()
                        .Where(info => info.TryGetAttribute(typeof(ConnectionStringParameterAttribute).FullName, out connectionStringParameterAttribute));


                    // Convert both lists into ConnectionStringParameter lists, combine
                    // the two lists, and then order them lexically while giving precedence
                    // to "required" parameters (those lacking a default value).
                    Dictionary<string, string> settings = this.ConnectionString.ToNonNullString().ParseKeyValuePairs();

                    return infoList.Select(item => GetParameter(item, settings))
                        .OrderByDescending(parameter => parameter.IsRequired)
                        .ThenBy(parameter => parameter.Name)
                        .ToList();
                }
            }

            // If the adapter type could not be found, we
            // return an empty List.
            return new List<ConnectionStringParameter>();
        }

        private ConnectionStringParameter GetParameter(PropertyInfo info, Dictionary<string, string> connectionStringParameters)
        {
            DefaultValueAttribute defaultValueAttribute;
            DescriptionAttribute descriptionAttribute;
            ConnectionStringParameter parameter = null;

            bool isRequired = false;
            object defaultValue = null;
            string description = null;


            isRequired = !info.TryGetAttribute(out defaultValueAttribute);
            defaultValue = isRequired ? null : defaultValueAttribute.Value;
            description = info.TryGetAttribute(out descriptionAttribute) ? descriptionAttribute.Description : string.Empty;

            // Create a brand new parameter to be returned.
            parameter = new ConnectionStringParameter
            {
                Name = info.Name,
                Description = description,
                Value = null,
                DefaultValue = defaultValue,
                IsRequired = isRequired
            };
            parameter.SetInfo(info);

            string value;
            parameter.Value = connectionStringParameters.TryGetValue(parameter.Name, out value) ? value : null;


            return parameter;
        }

        /// <summary>
        /// Updates the <see cref="ConnectionString"></see> Attribute.
        /// </summary>
        /// <param name="newParameters"> The List of <see cref="ConnectionStringParameter"/>. </param>
        public void UpdateConnectionString(List<ConnectionStringParameter> newParameters)
        {

            // The easiest way to update is to break the connection string into key
            // value pairs, update the value of the pair corresponding to the parameter
            // that fired the event, and then rejoin the key value pairs.
            Dictionary<string, string> settings = this.ConnectionString.ToNonNullString().ParseKeyValuePairs();

            newParameters.ForEach(item =>
            {
                // If it is Requires just update it
                if (item.IsRequired)
                {
                    if (settings.ContainsKey(item.Name))
                    {
                        settings[item.Name] = item.Value.ToNonNullString();
                    }
                    else
                    {
                        settings.Add(item.Name, item.Value.ToNonNullString());
                    }
                }

                // IF it is not Default Value just update it
                else if (item.Value != item.DefaultValue && item.Value.ToNonNullString() != "")
                {
                    if (settings.ContainsKey(item.Name))
                    {
                        settings[item.Name] = item.Value.ToNonNullString();
                    }
                    else
                    {
                        settings.Add(item.Name, item.Value.ToNonNullString());
                    }
                }
                // If it is Default Value or empty make sure it is not in the actual ConnectionString
                else
                {
                    if (settings.ContainsKey(item.Name))
                        settings.Remove(item.Name);
                }
            });

            // Build the new connection string and validate that it can still be parsed
            string connectionString = settings.JoinKeyValuePairs();


            this.ConnectionString = connectionString;

        }

        #endregion

    }

    /// <summary>
    /// Model for a key-value pair in an adapter connection string.
    /// This model is used by the Web UI
    /// </summary>
    public class ConnectionStringParameter
    {
        #region [ Members ]

        // Fields
        private PropertyInfo m_info;
        private string m_name;
        private string m_description;
        private object m_value;
        private object m_defaultValue;
        private bool m_isRequired;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Sets the <see cref="PropertyInfo"/> of the
        /// <see cref="GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute"/>
        /// associated with this <see cref="ConnectionStringParameter"/>.
        /// </summary>
        public void SetInfo(PropertyInfo info)
        {
            m_info = info;
        }
        

        /// <summary>
        /// Gets or sets the name of the <see cref="ConnectionStringParameter"/>
        /// which is either a key in the connection string or the name of a property in
        /// the adapter class.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of the <see cref="ConnectionStringParameter"/>
        /// obtained through the <see cref="SetInfo"/> using reflection. A property annotated with
        /// <see cref="GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute"/>
        /// must also define a <see cref="System.ComponentModel.DescriptionAttribute"/> for this
        /// to become populated.
        /// </summary>
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                m_description = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ConnectionStringParameter"/> as defined
        /// by either the connection string or the <see cref="DefaultValue"/> of the parameter.
        /// </summary>
        public object Value
        {
            get
            {
                return m_value ?? m_defaultValue;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Gets or sets the default value of the <see cref="ConnectionStringParameter"/>
        /// obtained through the <see cref="SetInfo"/> using reflection. A property annotated with
        /// <see cref="GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute"/> must
        /// also define a <see cref="System.ComponentModel.DefaultValueAttribute"/> for this to
        /// be populated.
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return m_defaultValue;
            }
            set
            {
                m_defaultValue = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this parameter is defined by a property
        /// that is annotated with the <see cref="System.ComponentModel.DefaultValueAttribute"/>.
        /// </summary>
        public bool IsRequired
        {
            get
            {
                return m_isRequired;
            }
            set
            {
                m_isRequired = value;
            }
        }



        /// <summary>
        /// Gets a list of enum types if this <see cref="ConnectionStringParameter"/>'s type is an enum.
        /// If it is not an enum, this returns null.
        /// </summary>
        public List<string> EnumValues
        {
            get
            {
                if (!IsEnum)
                    return null;

                return Enum.GetValues(m_info.PropertyType)
                    .Cast<object>()
                    .Select(obj => obj.ToString())
                    .ToList();
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the value of this parameter can be configured via a
        /// custom control. This determines whether the hyperlink that links to the custom configuration
        /// popup is visible.
        /// </summary>
        public bool IsCustom
        {
            get
            {
                try
                {
                    return (m_info != null) && (m_info.GetCustomAttribute<CustomConfigurationEditorAttribute>() != null);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the type of this <see cref="ConnectionStringParameter"/>
        /// is defined to be a <see cref="bool"/> in the adapter type. 
        /// </summary>
        public bool IsBoolean
        {
            get
            {
                return !IsCustom && (m_info != null) && (m_info.PropertyType == typeof(bool));
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the type of this <see cref="ConnectionStringParameter"/>
        /// is defined to be an enum in the adapter type. 
        /// </summary>
        public bool IsEnum
        {
            get
            {
                return !IsCustom && (m_info != null) && m_info.PropertyType.IsEnum;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the type of this <see cref="ConnectionStringParameter"/>
        /// is something other than a <see cref="bool"/> or an enum..
        /// </summary>
        public bool IsOther
        {
            get
            {
                return !IsCustom && !IsBoolean && !IsEnum;
            }
        }

        #endregion

    }
}
