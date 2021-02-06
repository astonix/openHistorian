#******************************************************************************************************
#  library.py - Gbtc
#
#  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
#
#  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
#  the NOTICE file distributed with this work for additional information regarding copyright ownership.
#  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
#  file except in compliance with the License. You may obtain a copy of the License at:
#
#      http://opensource.org/licenses/MIT
#
#  Unless agreed to in writing, the subject software distributed under the License is distributed on an
#  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
#  License for the specific language governing permissions and limitations.
#
#  Code Modification History:
#  ----------------------------------------------------------------------------------------------------
#  02/02/2021 - J. Ritchie Carroll
#       Generated original version of source code.
#
#******************************************************************************************************

from snapTypeBase import snapTypeBase
from encodingDefinition import encodingDefinition
from keyValueEncoderBase import keyValueEncoderBase
from common import static_init
from typing import Optional
from uuid import UUID

# Import SNAPdb types for manual registration
from historianKey import historianKey
from historianValue import historianValue
from historianKeyValueEncoder import historianKeyValueEncoder
from fixedSizeKeyValueEncoder import fixedSizeKeyValueEncoder

@static_init
class library:
    """
    Registered library of SNAPdb types.
    """

    @classmethod
    def static_init(cls):
        cls.typeNameIDMap = dict()
        cls.typeIDNameMap = dict()
        cls.guidEncoderMap = dict()

        # Register known SNAPdb key/value types. Future versions could dynamically scan
        # for modules inheriting `snapTypeBase` and automatically register them. For now
        # this class just manually registers `historianKey` and `historianValue` for use
        # by the `openHistorian` API
        cls.RegisterType(historianKey())
        cls.RegisterType(historianValue())

        # Register known SNAPdb key/value encoders. Future versions could dynamically scan
        # for modules inheriting `keyValueEncoderBase` and automatically register them. For
        # now this class just manually registers `historianKeyValueEncoder` and the generic
        # `fixedSizeKeyValueEncoder` for `historianKey` and `historianValue` for use by the
        # `openHistorian` API
        cls.RegisterEncoder(historianKeyValueEncoder())
        cls.RegisterEncoder(fixedSizeKeyValueEncoder(historianKey(), historianValue()))

    @classmethod
    def RegisterType(cls, snapType: snapTypeBase):
        typeName = type(snapType).__name__
        typeID = snapType.TypeID

        cls.typeNameIDMap[typeName] = typeID
        cls.typeIDNameMap[typeID] = typeName

    @classmethod
    def RegisterEncoder(cls, encoder: keyValueEncoderBase):
        if encoder.Definition.IsKeyValueEncoded:
            cls.guidEncoderMap[encoder.Definition.KeyValueEncodingMethod] = encoder
        else:
            raise RuntimeError("Separate key/value type encoding is not currently supported by Python API")

    @classmethod
    def LookupTypeName(cls, typeID: UUID) -> Optional[str]:
        return cls.typeIDNameMap[typeID]

    @classmethod
    def LookupTypeID(cls, typeName: str) -> Optional[UUID]:
        return cls.typeNameIDMap[typeName]

    @classmethod
    def LookupEncoder(cls, definition: encodingDefinition) -> Optional[keyValueEncoderBase]:
        if definition.IsKeyValueEncoded:
            return cls.guidEncoderMap[definition.KeyValueEncodingMethod]
        else:
            raise RuntimeError("Separate key/value type encoding is not currently supported by Python API")
