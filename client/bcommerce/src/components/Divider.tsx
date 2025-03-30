import React from 'react';
import type { IconProps } from '../types/icon-type';

type DividerVariant = 'button-icon';

interface DividerProps {
    variant?: DividerVariant;
    icon?: React.FC<IconProps>;
    isActive?: boolean;
    label?: string;
    onClick?: () => void;

}

const Divider: React.FC<DividerProps> = ({ variant = 'button - icon', icon: IconComponent, isActive = true, label = "", onClick }) => {

    return (
        <div className="pb-[90px]">
            <div className="relative mt-[90px] h-[1px]">
                {/* Linha com degradê nas pontas */}
                <div className="absolute top-0 left-[5%] right-[5%] h-[1px]">
                    <div
                        className="w-full h-full border-t border-dashed border-gray-300"
                        style={{
                            maskImage:
                                'linear-gradient(to right, transparent, white 30%, white 70%, transparent)',
                            WebkitMaskImage:
                                'linear-gradient(to right, transparent, white 30%, white 70%, transparent)',
                            maskRepeat: 'no-repeat',
                            maskSize: '100% 100%',
                        }}
                    />
                </div>

                {/* Ícone central com botão custom */}
                {variant === 'button-icon' && IconComponent ? (
                    <div className="absolute left-1/2 top-[-11px] transform -translate-x-1/2 z-10 flex flex-col items-center gap-1">
                        <button onClick={onClick}
                            className="w-6 h-6 rounded-full bg-[#111827] flex items-center justify-center shadow-lg cursor-pointer focus:outline-none transition transform active:scale-95">
                            <IconComponent isActive={isActive} color1="#FFF" />
                        </button>
                        <span className="text-xs text-center text-gray-700">{label}</span>
                    </div>
                ) : (
                    <div />
                )}
            </div>
        </div>
    );
};

export default Divider;


