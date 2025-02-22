import React from 'react'
import Search from '../components/Search'

const Collection: React.FC = () => {
    return (
        <div>
            <div>
                {/* FILTERS */}
                <div>
                    <Search />
                    <div>
                        <h5>Categories</h5>
                        <div>
                            {[]}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default Collection
