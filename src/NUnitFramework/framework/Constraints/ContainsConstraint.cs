// ****************************************************************
// Copyright 2002-2018, Charlie Poole
// This is free software licensed under the NUnit license, a copy
// of which should be included with this software. If not, you may
// obtain a copy at https://github.com/nunit-legacy/nunitv2.
// ****************************************************************

using System;
using System.Collections;
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif

namespace NUnit.Framework.Constraints
{
    // TODO Needs tests
    /// <summary>
    /// ContainsConstraint tests a whether a string contains a substring
    /// or a collection contains an object. It postpones the decision of
    /// which test to use until the type of the actual argument is known.
    /// This allows testing whether a string is contained in a collection
    /// or as a substring of another string using the same syntax.
    /// </summary>
    public class ContainsConstraint : Constraint
    {
        readonly object expected;
        Constraint realConstraint;
        bool ignoreCase;

#if CLR_2_0 || CLR_4_0
        private List<EqualityAdapter> equalityAdapters = new List<EqualityAdapter>();
#else
        private ArrayList equalityAdapters = new ArrayList();
#endif

        private Constraint RealConstraint
        {
            get 
            {
                if ( realConstraint == null )
                {
                    if (actual is string)
                    {
                        StringConstraint constraint = new SubstringConstraint((string)expected);
                        if (this.ignoreCase)
                            constraint = constraint.IgnoreCase;
                        this.realConstraint = constraint;
                    }
                    else
                    {
                        CollectionItemsEqualConstraint constraint = new CollectionContainsConstraint(expected);
                        
                        foreach (EqualityAdapter adapter in equalityAdapters)
                            constraint = constraint.Using(adapter);
                            
                        this.realConstraint = constraint;
                    }
                }
                
                return realConstraint;
            }
            set 
            { 
                realConstraint = value; 
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsConstraint"/> class.
        /// </summary>
        /// <param name="expected">The expected.</param>
        public ContainsConstraint( object expected ) : base(expected)
        {
            this.expected = expected;
        }

        /// <summary>
        /// Flag the constraint to ignore case and return self.
        /// </summary>
        public ContainsConstraint IgnoreCase
        {
            get { this.ignoreCase = true; return this; }
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;
            return this.RealConstraint.Matches( actual );
        }

        /// <summary>
        /// Write the constraint description to a MessageWriter
        /// </summary>
        /// <param name="writer">The writer on which the description is displayed</param>
        public override void WriteDescriptionTo(MessageWriter writer)
        {
            this.RealConstraint.WriteDescriptionTo(writer);
        }

        /// <summary>
        /// Flag the constraint to use the supplied IComparer object.
        /// </summary>
        /// <param name="comparer">The IComparer object to use.</param>
        /// <returns>Self.</returns>
        public ContainsConstraint Using(IComparer comparer)
        {
            return AddAdapter(EqualityAdapter.For(comparer));
        }

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Flag the constraint to use the supplied IComparer object.
        /// </summary>
        /// <param name="comparer">The IComparer object to use.</param>
        /// <returns>Self.</returns>
        public ContainsConstraint Using<T>(IComparer<T> comparer)
        {
            return AddAdapter(EqualityAdapter.For(comparer));
        }

        /// <summary>
        /// Flag the constraint to use the supplied Comparison object.
        /// </summary>
        /// <param name="comparer">The IComparer object to use.</param>
        /// <returns>Self.</returns>
        public ContainsConstraint Using<T>(Comparison<T> comparer)
        {
            return AddAdapter(EqualityAdapter.For(comparer));
        }

        /// <summary>
        /// Flag the constraint to use the supplied IEqualityComparer object.
        /// </summary>
        /// <param name="comparer">The IComparer object to use.</param>
        /// <returns>Self.</returns>
        public ContainsConstraint Using(IEqualityComparer comparer)
        {
            return AddAdapter(EqualityAdapter.For(comparer));
        }

        /// <summary>
        /// Flag the constraint to use the supplied IEqualityComparer object.
        /// </summary>
        /// <param name="comparer">The IComparer object to use.</param>
        /// <returns>Self.</returns>
        public ContainsConstraint Using<T>(IEqualityComparer<T> comparer)
        {
            return AddAdapter(EqualityAdapter.For(comparer));
        }
#endif

        #region Helper Methods

        private ContainsConstraint AddAdapter(EqualityAdapter adapter)
        {
            this.equalityAdapters.Add(adapter);
            return this;
        }

        #endregion
    }
}
