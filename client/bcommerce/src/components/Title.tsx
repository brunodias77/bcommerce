import React from 'react'

interface TitleProps {
    title: string;
    subtitle: string;
    content?: string;
    styles?: string;
    titleStyles?: string;
    pStyles?: string;
}

const Title: React.FC<TitleProps> = ({ title, subtitle, content, styles, titleStyles, pStyles }) => {
    return (
        <div className={`${styles} pb-1`}>
            <h2 className={`${titleStyles} text-primary text-[25px] leading-tight md:text-[35px] md:leading-[1.3] mb-4 font-bold`}>
                {title}
                <span className='text-[#2D2926] !font-light underline'>{subtitle}</span>
            </h2>
            <p className={`${pStyles} hidden`}>{content}</p>
        </div>
    )
}

export default Title;
