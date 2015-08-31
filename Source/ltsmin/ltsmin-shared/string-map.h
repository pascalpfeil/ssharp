// Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013, 2014, 2015 Formal Methods and Tools, University of Twente
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
//  * Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 
//  * Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 
//  * Neither the name of the University of Twente nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

/**
\file string-map.h
\brief Create and call string to string maps.
*/

/**
\brief Handle for a string to string mapping.
*/
typedef struct string_string_map *string_map_t;

/**
\brief Handle for a set of strings.
*/
typedef struct string_set *string_set_t;

/**
\brief Create a map that from a Shell Wild specification.
*/
extern string_map_t SSMcreateSWP(const char* swp_spec);

/**
\brief Find the mapped value for the given input.
*/
extern char* SSMcall(string_map_t map, const char*input);

/**
\brief Create a set from a Shell Wild specification.
*/
extern string_set_t SSMcreateSWPset(const char* swp_spec);

/**
\brief Test if the input is member of the set.
*/
extern int SSMmember(string_set_t set, const char*input);