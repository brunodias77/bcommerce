import React from 'react'
import { blogs } from "../assets/products/data";

const Blog: React.FC = () => {
    return (
        <section className='max-padd-container pb-16'>
            <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5'>
                {blogs.slice(0, 4).map((blog) => (
                    <div key={blog.title} className='relative'>
                        <img src={blog.image} alt="image of blog" className='rounded-xl' />
                        {/* INFO */}
                        <div>
                            <p className='text-[14px] font-[500] mt-6'>{blog.category}</p>
                            <h5 className='h4 pr-4 mb-1'>{blog.title}</h5>
                            <p>Lorem ipsum dolor sit amet, consectetur adipisicing elit.
                                Distinctio ipsam ullam nulla quod dolore veniam iste necessitatibus, eligendi sapiente eius, maxime esse voluptas quisquam! Repellendus eum veritatis deserunt laudantium amet!
                            </p>
                            <button className="underline mt-2 text-[14px] font-[700]">continue reading</button>
                        </div>
                    </div>
                ))}

            </div>
        </section>
    )
}

export default Blog;
